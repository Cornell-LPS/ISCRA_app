from fastapi import FastAPI
from pydantic import BaseModel
from pathlib import Path
import base64
#from oes_code import *
import uvicorn
from oes_io import *
from plot import *
from calibrate import *


app = FastAPI()

neon_path = None
shot_path = None
output_path = None
fiber_path = None
neon_image = None
shot_image = None
peaks_pressed = []
neon_wave = []
done = False
active_peak = None
peak_arr = None
peak_arr_loc = None
wavelength_nm = None
center_col = None
fiber_boxes = None
y_regions = None
x0 = None
x1 = None
n_fibers = None

class FileSaveFibers(BaseModel):
    output_path: str

class FileSaveLocations(BaseModel):
    output_path: str

class FileSaveEnergies(BaseModel):
    output_path: str

class EditOpenNeon(BaseModel):
    neon_path: Path

class ButtonDone(BaseModel):
    done: bool

class EditOpenOes(BaseModel):
    shot_path: Path

class ProcessAutoFibers(BaseModel):
    n_fibers: int

class ProcessImportFibers(BaseModel):
    fiber_path: Path
    expected_n_fibers: int | None = None

class ProcessCalibrate(BaseModel):
    output_path: str

class ProcessFindNeon(BaseModel):
    clicked_pixel: float | None = None
    clicked_neon: float | None = None
    done: bool = False

class RadioMap(BaseModel):
    pass

class RadioFiberBoxes(BaseModel):
    pass

class RadioFibersPlot(BaseModel):
    pass

class RadioNeonPlot(BaseModel):
    pass

class NeonUpdatedPlot(BaseModel):
    pass

class RadioMedianFibers(BaseModel):
    pass

class RadioWavelengthPixel(BaseModel):
    pass

class ShotNumber(BaseModel):
    shot_number: int | None = None

def fiber_boxes_list(fiber_boxes):
    return [{"x0": int(x), "y0": int(y), "width": int(w), "height": int(h), "x1": int(x + w), "y1": int(y + h)} for x, y, w, h in fiber_boxes]

def y_list(y_regions):
    return [{"y0": int(y0), "y1": int(y1)} for y0, y1 in y_regions]

def find_peaks_for_gui(neon_image):
    spec = neon_image.sum(axis=0)
    peaks, _ = find_peaks(spec, prominence=np.percentile(spec, 100) * 0.01, distance=10)
    peak_arr = np.sort(peaks)
    peak_arr_loc = [{"X": float(peak), "Y": float(spec[peak])} for peak in peak_arr]
    return peak_arr, peak_arr_loc

@app.post("/save-fiber-spectrum")
def run_save_fibers_spectrum(req: FileSaveFibers):
    global output_path
    output_path = Path(req.output_path)
    saved_spectrum_fibers = save_fibers_to_csv(shot_image, wavelength_nm, x0, x1, y_regions, output_path, shot_path)
    return {"status": "Fiber spectrum saved.", "saved_spectrum_fibers": str(saved_spectrum_fibers)}

@app.post("/save-fiber-box-coordinates")
def run_save_fiber_coordinates(req: FileSaveLocations):
    global output_path
    output_path = Path(req.output_path)
    coordinates = save_fibers_coordinates(fiber_boxes, output_path, shot_path)
    return {"status": "ok", "fiber_coordinates": str(coordinates), "fiber_boxes": fiber_boxes_list(fiber_boxes)}

@app.post("/save-photon-energy")
def run_save_photon_energy(req: FileSaveEnergies):
    global output_path
    output_path = Path(req.output_path)
    photons = photon_energy_save(shot_image, wavelength_nm, fiber_boxes, output_path, shot_path)
    return {"status": "ok", "photon_energy_file": str(photons)}

@app.post("/open-lamp-spectrum")
def run_edit_open_lamp(req: EditOpenNeon):
    global neon_path, neon_image, peaks_pressed, neon_wave, done, active_peak, peak_arr, peak_arr_loc
    neon_path = req.neon_path
    neon_image = load_image(neon_path)
    peaks_pressed = []
    neon_wave = []
    done = False
    active_peak = None
    peak_arr, peak_arr_loc = find_peaks_for_gui(neon_image)
    return {"status": "ok", "message": "Lamp spectrum file successfully uploaded.", "neon_path": str(neon_path), "neon_shape": list(neon_image.shape), "peak_arr_loc": peak_arr_loc}

@app.post("/is-done")
def run_button_done(req: ButtonDone):
    done = True
    return {"status": "ok", "done": done}

@app.post("/open-oes-file")
def run_edit_open_oes(req: EditOpenOes):
    global shot_path, shot_image
    shot_path = req.shot_path
    shot_image = load_image(shot_path)
    return {"status": "ok", "message": "OES file successfully uploaded.", "shot_path": str(shot_path), "shot_shape": list(shot_image.shape)}

@app.post("/auto-find-fiber-box-locations")
def run_process_auto_find(req: ProcessAutoFibers):
    global fiber_boxes, y_regions, x0, x1, n_fibers
    n_fibers = req.n_fibers
    fiber_boxes, y_regions, x0, x1 = auto_find_fiber_boxes(shot_image, n_fibers)
    image = base64.b64encode(plot_fiber_boxes(shot_image, fiber_boxes, shot_number)).decode("utf-8")
    return {"status": "Fiber box locations found.", "fiber_boxes": fiber_boxes_list(fiber_boxes), "x0": int(x0), "x1": int(x1), "y_regions": y_list(y_regions), "image": image}

@app.post("/upload-fiber-box-coordinates")
def run_process_upload_coordinates(req: ProcessImportFibers):
    global fiber_path, fiber_boxes, y_regions, x0, x1, n_fibers
    fiber_path = req.fiber_path
    fiber_boxes = upload_fibers_coordinates(shot_image, req.expected_n_fibers, fiber_path)

    if isinstance(fiber_boxes, dict):
        return fiber_boxes

    n_fibers = len(fiber_boxes)
    y_regions = [(y, y + h) for x, y, w, h in fiber_boxes]
    x0 = min(x for x, y, w, h in fiber_boxes)
    x1 = max(x + w for x, y, w, h in fiber_boxes)

    image = base64.b64encode(plot_fiber_boxes(shot_image, fiber_boxes, shot_number)).decode("utf-8")
    return {"status": "ok", "fiber_boxes": fiber_boxes_list(fiber_boxes), "x0": int(x0), "x1": int(x1), "y_regions": y_list(y_regions), "image": image}

@app.post("/calibrate")
def run_process_calibrate(req: ProcessCalibrate):
    global output_path, wavelength_nm, center_col, shot_image
    output_path = Path(req.output_path)
    wavelength_nm, message = calibrate(neon_image, [peaks_pressed], neon_wave)
    center_col = neon_image.shape[1] // 2
    shot_image, wavelength_nm, calibrated_csv = calibrate_shot(shot_path, center_col, wavelength_nm, output_path)
    return {"status": "ok", "message": message["message"], "center_col": int(center_col), "center_wavelength": float(wavelength_nm[center_col]), "calibrated_csv": str(calibrated_csv)}

@app.post("/lamp-emission-lines")
def run_process_lamp_emission(req: ProcessFindNeon):
    global done
    if req.done:
        done = True
        active_peak = None
        return {"status": "ok", "message": "Successfully assigned the selected peaks.", "peaks_pressed": peaks_pressed, "neon_wave": neon_wave}

    if req.clicked_pixel is not None and req.clicked_neon is not None:
        peaks_pressed.append(float(req.clicked_pixel))
        neon_wave.append(float(req.clicked_neon))
        active_peak = None
        return {"status": "ok", "message": f"Successfully paired {req.clicked_pixel} with {req.clicked_neon} nm", "peaks_pressed": peaks_pressed, "neon_wave": neon_wave}

    if req.clicked_pixel is not None:
        active_peak = float(req.clicked_pixel)
        return {"status": "active", "message": "Select a lamp emission line for this peak.", "peaks_pressed": peaks_pressed, "neon_wave": neon_wave}

    if req.clicked_neon is not None and active_peak is not None:
        peaks_pressed.append(float(active_peak))
        neon_wave.append(float(req.clicked_neon))
        active_peak = None
        return {"status": "ok", "message": f"Successfully paired peak with {req.clicked_neon} nm", "peaks_pressed": peaks_pressed, "neon_wave": neon_wave}

    return {"status": "notok", "message": "No peak or neon wavelength was selected.", "peaks_pressed": peaks_pressed, "neon_wave": neon_wave}

@app.post("/plot-neon")
def run_plot_neon(req: RadioNeonPlot):
    global peak_arr_loc

    if neon_image is None:
        return {"status": "notok", "message": "Open a lamp spectrum first.", "peak_arr": [], "peak_arr_loc": [], "image": None}

    image, peak_arr_loc = plot_lamp_spec(neon_image, peak_arr, shot_number)
    image = base64.b64encode(image).decode("utf-8")
    return {"status": "Lamp spectrum successfully plotted.", "message": "Lamp spectrum successfully plotted.", "neon_shape": list(neon_image.shape), "peak_arr": [float(x) for x in peak_arr], "peak_arr_loc": peak_arr_loc, "image": image}

@app.post("/plot-neon-updated")
def run_plot_neon_updated(req: NeonUpdatedPlot):
    global peak_arr_loc
    image = plot_lamp_spec_updated(neon_image, wavelength_nm, peak_arr, shot_number)
    image = base64.b64encode(image).decode("utf-8")
    return {"status": "ok", "message": "Success.", "neon_shape": list(neon_image.shape), "peak_arr": [float(x) for x in peak_arr], "peak_arr_loc": [], "image": image}

@app.post("/show-fiber-boxes")
def run_show_fiber_boxes(req: RadioFiberBoxes):
    image = base64.b64encode(plot_fiber_boxes(shot_image, fiber_boxes, shot_number)).decode("utf-8")
    return {"status": "ok", "shown": "fiber_boxes", "fiber_boxes": fiber_boxes_list(fiber_boxes), "image": image}

@app.post("/show-fibers-plot")
def run_show_fibers_plot(req: RadioFibersPlot):
    image = base64.b64encode(plot_fibers(shot_image, wavelength_nm, fiber_boxes, shot_number)).decode("utf-8")
    return {"status": "ok", "shown": "fibers_plot", "image": image}

@app.post("/show-median-fibers")
def run_show_median_fibers(req: RadioMedianFibers):
    image, peaks = plot_median_fibers(shot_image, wavelength_nm, fiber_boxes, shot_number)
    image = base64.b64encode(image).decode("utf-8")
    return {"status": "ok", "shown": "median_fibers", "peaks": [float(x) for x in peaks], "image": image}

@app.post("/show-wavelength-pixel")
def run_show_wavelength_pixel(req: RadioWavelengthPixel):
    image, equation = plot_wavelength_pixel(peaks_pressed, neon_wave, shot_number)
    image = base64.b64encode(image).decode("utf-8")
    return {"status": "ok", "shown": "wavelength_pixel", "equation": equation, "image": image}

@app.post("/shot-number")
def run_shot_number_routine(req: ShotNumber):
    global shot_number 
    shot_number = req.shot_number
    return {"status": "Shot number entered successfully.", "shot_number" : shot_number}


@app.get("/health")
def run_health():
    return {"status": "ok"}

if __name__ == "__main__":
    uvicorn.run(app, host="127.0.0.1", port=8000)