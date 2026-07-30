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


def plot_lamp_spec(neon_image, peak_arr, shot_number: int):

    spec = neon_image.sum(axis=0)
    fig, ax = plt.subplots(figsize=(12, 7), dpi=100)
    ax.plot(np.arange(len(spec)), spec)
    ax.scatter(peak_arr, spec[peak_arr])
    ax.set_title(f"{shot_number} Lamp Emission Spectrum")
    ax.set_xlabel("Horizontal Pixel")
    ax.set_ylabel("Intensity (Counts)")
    plt.tight_layout()
    fig.canvas.draw()

    peak_arr_loc = []

    for peak in peak_arr:
        image_x, image_y = ax.transData.transform((peak, spec[peak]))
        image_y = fig.canvas.get_width_height()[1] - image_y
        peak_arr_loc.append({"X": float(image_x), "Y": float(image_y), "Pixel": float(peak)})

    image_buffer = BytesIO()
    fig.savefig(image_buffer, format="png", dpi=100)
    plt.close(fig)
    image_buffer.seek(0)
    return image_buffer.getvalue(), peak_arr_loc



def plot_fiber_boxes(shot_image, fiber_boxes, shot_number: int):

    fig, ax = plt.subplots(figsize=(12, 7))
    image_plot = ax.imshow(shot_image, aspect="auto", origin="lower", cmap="viridis")

    for i, box in enumerate(fiber_boxes):
        x, y, w, h = box
        rect = Rectangle((x, y), w, h, linewidth=2, edgecolor="red", facecolor="none")
        ax.add_patch(rect)
        ax.text(x + 5, y + h / 2, str(i + 1), color="white", fontsize=8, verticalalignment="center")

    ax.set_title(f"{shot_number} OES Fiber Boxes")
    ax.set_xlabel("Horizontal Pixel")
    ax.set_ylabel("Vertical Pixel")
    cbar = fig.colorbar(image_plot, ax=ax)
    cbar.set_label("Intensity (Counts)")
    plt.tight_layout()

    image_buffer = BytesIO()
    fig.savefig(image_buffer, format="png", dpi=100)
    plt.close(fig)
    image_buffer.seek(0)
    return image_buffer.getvalue()

def plot_fibers(shot_image, wavelength_nm, fiber_boxes, shot_number: int):

    fig, ax = plt.subplots(figsize=(12, 7))

    for i, box in enumerate(fiber_boxes):
        x, y, w, h = box
        fiber_img = shot_image[y:y+h, x:x+w]
        fiber_signal = fiber_img.mean(axis=0)
        fiber_wavelength = wavelength_nm[x:x+w]
        ax.plot(fiber_wavelength, fiber_signal, label=f"Fiber {i + 1}", linewidth=1)

    ax.set_title(f"{shot_number} OES Fiber Spectra")
    ax.set_xlabel("Wavelength (nm)")
    ax.set_ylabel("Intensity (Counts)")
    ax.legend(fontsize=8)
    plt.tight_layout()

    image_buffer = BytesIO()
    fig.savefig(image_buffer, format="png", dpi=100)
    plt.close(fig)
    image_buffer.seek(0)
    return image_buffer.getvalue()


def plot_lamp_spec_updated(neon_image, wavelength_nm, peak_arr, shot_number: int):

    spec = neon_image.sum(axis=0)
    fig, ax = plt.subplots(figsize=(12, 7), dpi=100)
    ax.plot(wavelength_nm, spec)
    ax.set_title(f"{shot_number} Calibrated Lamp Emission Spectrum")
    ax.set_xlabel("Wavelength (nm)")
    ax.set_ylabel("Intensity (Counts)")
    plt.tight_layout()

    image_buffer = BytesIO()
    fig.savefig(image_buffer, format="png", dpi=100)
    plt.close(fig)
    image_buffer.seek(0)
    return image_buffer.getvalue()

def plot_median_fibers(shot_image, wavelength_nm, fiber_boxes, shot_number: int):

    fiber_signals = []

    for x, y, w, h in fiber_boxes:
        fiber_signal = shot_image[y:y+h, x:x+w].mean(axis=0)
        fiber_signals.append(fiber_signal)

    median_signal = np.median(np.asarray(fiber_signals), axis=0)
    smoothed_signal = savgol_filter(median_signal, 11, 3)
    peaks, _ = find_peaks(smoothed_signal, prominence=np.percentile(smoothed_signal, 100) * 0.01, distance=10)

    x, _, w, _ = fiber_boxes[0]
    fiber_wavelength = wavelength_nm[x:x+w]

    fig, ax = plt.subplots(figsize=(12, 7))
    ax.plot(fiber_wavelength, median_signal, label="Median", linewidth=1)
    ax.plot(fiber_wavelength, smoothed_signal, label="Savitzky-Golay", linewidth=1)
    ax.scatter(fiber_wavelength[peaks], smoothed_signal[peaks])
    ax.set_title(f"{shot_number} Median Fiber Spectrum")
    ax.set_xlabel("Wavelength (nm)")
    ax.set_ylabel("Intensity (Counts)")
    ax.legend(fontsize=8)
    plt.tight_layout()

    image_buffer = BytesIO()
    fig.savefig(image_buffer, format="png", dpi=100)
    plt.close(fig)
    image_buffer.seek(0)
    return image_buffer.getvalue(), fiber_wavelength[peaks]


def plot_wavelength_pixel(peaks_pressed, neon_wave, shot_number: int):

    pixel_peaks = np.asarray(peaks_pressed)
    neon_wave = np.asarray(neon_wave)
    coeff = np.polyfit(pixel_peaks, neon_wave, deg=1)
    order = np.argsort(pixel_peaks)
    equation = f"Wavelength = {coeff[0]:.6f} * Pixel + {coeff[1]:.6f}"

    fig, ax = plt.subplots(figsize=(12, 7))
    ax.scatter(pixel_peaks, neon_wave)
    ax.plot(pixel_peaks[order], np.polyval(coeff, pixel_peaks[order]))
    ax.set_title(f"Shot {shot_number} Wavelength vs Pixel Calibration")
    ax.set_xlabel("Horizontal Pixel")
    ax.set_ylabel("Wavelength (nm)")
    plt.tight_layout()

    image_buffer = BytesIO()
    fig.savefig(image_buffer, format="png", dpi=100)
    plt.close(fig)
    image_buffer.seek(0)
    return image_buffer.getvalue(), equation
