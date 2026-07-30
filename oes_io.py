import numpy as np
from pathlib import Path
import sif_parser
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
from scipy.signal import find_peaks, savgol_filter
import pandas as pd
from matplotlib.patches import Rectangle
from io import BytesIO

def load_image(sif_path: Path):

    with open(sif_path, "rb") as f:
        obj = sif_parser.np_open(f)
    arr = np.asarray(obj[0] if isinstance(obj, tuple) else getattr(obj, "data", obj))
    return arr[0]


def sif_to_csv(shot_path: Path, output_dir: Path, wavelength_nm):

    img = load_image(shot_path)
    n_rows, n_cols = img.shape
    wavelength_nm = np.asarray(wavelength_nm).reshape(-1)

    output_dir.mkdir(parents=True, exist_ok=True)
    out_csv = output_dir / f"{shot_path.stem}_spectroscopy.csv"
    df = pd.DataFrame(img)
    df.insert(0, "pixel_row", np.arange(n_rows))
    footer = pd.DataFrame([[np.nan, *wavelength_nm.tolist()]], columns=df.columns)
    pd.concat([df, footer], ignore_index=True).to_csv(out_csv, index=False, header=False)
    return out_csv



def save_fibers_coordinates(fiber_boxes, output_dir: Path, shot_path: Path):

    output_dir.mkdir(parents=True, exist_ok=True)

    rows = []
    for fiber_number, (x, y, w, h) in enumerate(
        fiber_boxes,
        start=1
    ):
        rows.append({
            "Fiber": fiber_number,
            "x0": int(x),
            "x1": int(x + w),
            "y0": int(y),
            "y1": int(y + h),
            "width": int(w),
            "height": int(h)
        })

    coordinates_df = pd.DataFrame(rows)

    out_csv = (output_dir/ f"{shot_path.stem}_fiber_coordinates.csv")
    coordinates_df.to_csv(out_csv, index=False)
    print("Saved fiber coordinates:", out_csv)
    return out_csv

#fiber_path is given by the user from c# part
#expected_n_fibers is given by the user from c# part. do we even need this?
def upload_fibers_coordinates(shot_image, expected_n_fibers, fiber_path):

    if not fiber_path.exists():
        return{"status": "notok", "message": "File does not exist."}

    coordinates_df = pd.read_csv(fiber_path)

    required_columns = {
        "x0",
        "y0",
        "width",
        "height"
    }

    missing_columns = (
        required_columns
        - set(coordinates_df.columns)
    )

    if missing_columns:
        return{"status": "notok", "message": "The file format is invalid."}

    fiber_boxes = []

    n_rows, n_cols = shot_image.shape

    for row_number, row in coordinates_df.iterrows():

        x = int(row["x0"])
        y = int(row["y0"])
        w = int(row["width"])
        h = int(row["height"])

        if (
            x < 0
            or y < 0
            or x + w > n_cols
            or y + h > n_rows
        ):
            return{"status": "notok", "message": f"Fiber box in row {row_number + 1} " +
            "is outside the shot image."}

        fiber_boxes.append((x, y, w, h))

    fiber_boxes = sorted(fiber_boxes, key=lambda box: box[1])

    if (expected_n_fibers is not None and len(fiber_boxes) != expected_n_fibers):
        return {"status": "notok", "message": f"The file contains {len(fiber_boxes)} fibers, " f"but you entered {expected_n_fibers}."}

    return fiber_boxes


def photon_energy_save(shot_image, wavelength_nm, fiber_boxes, output_dir: Path, shot_path: Path):

    fiber_signals = []

    for x, y, w, h in fiber_boxes:
        fiber_signal = shot_image[y:y+h, x:x+w].mean(axis=0)
        fiber_signals.append(fiber_signal)

    intensity = np.median(np.asarray(fiber_signals), axis=0)

    x, _, w, _ = fiber_boxes[0]
    wavelength = np.asarray(wavelength_nm[x:x+w])

    photon_energy_ev = 1239.841984332 / wavelength
    order = np.argsort(photon_energy_ev)

    output_dir.mkdir(parents=True, exist_ok=True)
    out_txt = output_dir / f"{shot_path.stem}_photon_energy.txt"

    np.savetxt(
        out_txt,
        np.column_stack((
            photon_energy_ev[order],
            intensity[order]
        )),
        delimiter="\t",
        header="Photon_energy_eV\tIntensity_counts",
        comments=""
    )

    print("Saved photon-energy spectrum:", out_txt)
    return out_txt

def save_fibers_to_csv(shot_image, wavelength_nm, x0, x1, y_regions, output_dir: Path, shot_path: Path):

    data_dict = {"Wavelength_nm": wavelength_nm[x0:x1]}

    for idx, (y0, y1) in enumerate(sorted(y_regions, key=lambda r: r[0]), start=1):
        fiber_slice = shot_image[y0:y1, x0:x1]
        fiber_profile = fiber_slice.mean(axis=0)
        data_dict[f"Fiber_{idx}"] = fiber_profile

    final_df = pd.DataFrame(data_dict)
    output_dir.mkdir(parents=True, exist_ok=True)
    out_csv = output_dir / f"{shot_path.stem}_fibers_data.csv"
    final_df.to_csv(out_csv, index=False)

    print("Saved final CSV")
    return out_csv
