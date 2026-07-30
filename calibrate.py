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
from oes_io import load_image

def peak_detection(neon_img):
    spec = neon_img.sum(axis=0)
    peaks, _ = find_peaks(spec, prominence=np.percentile(spec, 100) * 0.01, distance=10)
    peak_arr = np.sort(peaks)
    peak_arr_loc = []
    for peak in peak_arr:
        peak_arr_loc.append({
            "X": float(peak),
            "Y": float(spec[peak])
        })

    return peak_arr, peak_arr_loc

def process_neon_pressed(neon_img):
    peaks_pressed = []
    neon_wave = []
    done = False
    active_peak = None
    peak_arr, peak_arr_loc = peak_detection(neon_img)
    return peaks_pressed, neon_wave, done, active_peak, peak_arr, peak_arr_loc


def calibrate(neon_img, peaks_pressed, neon_wave):

    pixel_peaks = np.asarray(peaks_pressed[0])
    neon_wave = np.asarray(neon_wave)
    coeff = np.polyfit(pixel_peaks, neon_wave, deg=1)
    pixels = np.arange(neon_img.shape[1])
    wavelength_nm = np.polyval(coeff, pixels)
    center_col = neon_img.shape[1] // 2
    calculated_center_wavelength = wavelength_nm[center_col]

    return wavelength_nm, {"status": "ok", "message": f"Center wavelength is: {calculated_center_wavelength}"}


#clicked pixel is what c# sends to python and clicked_neon as well
def entering_spectrum(done, clicked_pixel, peak_arr, clicked_neon = None):

    global active_peak
    global peaks_pressed
    global neon_wave

    if not done:
        if active_peak is None:

            clicked_pixel = tuple(clicked_pixel)
            valid_peaks = {tuple(p) for p in peak_arr}

            if clicked_pixel in valid_peaks:
                peaks_pressed.append(clicked_pixel)
                active_peak = True
                return {"status": "ok", "message": "Select a lamp emission line for this peak."}
            else:
                return {"status": "notok", "message": "No valid peak was selected."}
        else:

            if clicked_neon is not None:
                neon_wave.append(clicked_neon)
                active_peak = None
                return{"status": "ok", "message": f"Successfully paired {clicked_pixel} with {clicked_neon}nm"}
            else:
                return {"status": "notok", "message": "No emission line was assigned to the peak."}

    if done:
        active_peak = None
        return peaks_pressed, neon_wave


def calibrate_shot(shot_path: Path, center_col: int, wavelength_nm, output_dir: Path):

    shot_image = load_image(shot_path)
    n_rows, n_cols = shot_image.shape

    wavelength_nm = np.asarray(wavelength_nm).reshape(-1)

    print("Shot image shape:", shot_image.shape)
    print("Center column:", center_col)
    print("Wavelength shift at center column:", wavelength_nm[center_col])

    output_dir.mkdir(parents=True, exist_ok=True)

    out_csv = output_dir / f"{shot_path.stem}_calibrated_shot.csv"

    df = pd.DataFrame(shot_image)
    df.insert(0, "pixel_row", np.arange(n_rows))

    footer = pd.DataFrame(
        [[np.nan, *wavelength_nm.tolist()]],
        columns=df.columns
    )

    calibrated_df = pd.concat([df, footer], ignore_index=True)
    calibrated_df.to_csv(out_csv, index=False, header=False)
    print("Saved calibrated shot:", out_csv)
    return shot_image, wavelength_nm, out_csv


def regions_from_mask(mask):
    idx = np.where(mask)[0]

    if len(idx) == 0:
        return []

    splits = np.where(np.diff(idx) > 1)[0] + 1
    groups = np.split(idx, splits)

    regions = []
    for g in groups:
        regions.append((int(g[0]), int(g[-1]) + 1))

    return regions


def smooth_profile(profile, window=15):
    kernel = np.ones(window) / window
    return np.convolve(profile, kernel, mode="same")

#n_fibers get from c# user
def auto_find_fiber_boxes(shot_image, n_fibers):

    img = np.asarray(shot_image)

    # Remove the image background
    img_bg_removed = img - np.percentile(img, 5)
    img_bg_removed[img_bg_removed < 0] = 0

    # Find the horizontal extent of the spectrum
    col_profile = img_bg_removed.mean(axis=0)
    col_profile = smooth_profile(col_profile, window=25)

    col_threshold = np.percentile(col_profile, 20) + 0.02 * (
        np.percentile(col_profile, 99)
        - np.percentile(col_profile, 20)
    )

    x_regions = regions_from_mask(col_profile > col_threshold)

    if len(x_regions) == 0:
        raise ValueError(
            "Could not automatically find the bright x-region."
        )

    x0, x1 = max(
        x_regions,
        key=lambda region: region[1] - region[0]
    )

    cropped = img_bg_removed[:, x0:x1]

    row_profile = cropped.mean(axis=1)
    row_profile = smooth_profile(row_profile, window=9)

    row_threshold = np.percentile(row_profile, 20) + 0.35 * (
        np.percentile(row_profile, 99)
        - np.percentile(row_profile, 20)
    )

    y_regions = regions_from_mask(row_profile > row_threshold)

    y_regions = [
        (y0, y1)
        for y0, y1 in y_regions
        if y1 - y0 > 5
    ]

    if len(y_regions) < n_fibers:
        raise ValueError(
            f"Only found {len(y_regions)} fiber regions, "
            f"expected {n_fibers}."
        )

    scored_regions = [
        (row_profile[y0:y1].mean(), y0, y1)
        for y0, y1 in y_regions
    ]

    scored_regions = sorted(
        scored_regions,
        reverse=True
    )[:n_fibers]

    scored_regions = sorted(
        scored_regions,
        key=lambda region: region[1]
    )

    fiber_boxes = []

    for _, y0, y1 in scored_regions:
        y0 = max(0, y0 - 2)
        y1 = min(img.shape[0], y1 + 2)

        fiber_boxes.append(
            (x0, y0, x1 - x0, y1 - y0)
        )

    print("Auto-detected fiber boxes:")

    for i, (x, y, w, h) in enumerate(fiber_boxes):
        print(
            f"Fiber {i + 1}: "
            f"x0={x}, x1={x + w}, "
            f"y0={y}, y1={y + h}, height={h}"
        )
    return fiber_boxes, y_regions, x0, x1
