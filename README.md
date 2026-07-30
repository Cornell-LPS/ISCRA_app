# ISCRA

Integrated Spectroscopy CalibRation Algorithm

A Windows desktop application for processing optical emission spectroscopy
(OES) data.

The application combines a C# Windows Forms interface with a Python FastAPI
backend for spectrum processing, wavelength calibration, visualization of results, and
data export.

## Features

- Open `.sif` lamp and OES files
- Automatically detect or import fiber-optic box regions
- Detect calibration lamp peaks
- Interactive assignment of known wavelengths for the lamp peaks
- Calculate wavelength vs pixel calibration and obtain best fit
- Display calibrated fiber spectra
- Display median of fiber spectra and locate prominent OES lines
- Export spectra, fiber coordinates, and photon-energy data

## Installing the Application (.exe file)

1. Download the latest Windows `.zip` from the repository's **Releases** page.
2. Extract the complete folder.
3. Keep all included files and folders together.
4. Run `ISCRA_app.exe`.
   
The application starts its packaged Python backend automatically.

> Windows may display a SmartScreen warning because the application is not
> currently code-signed. Only run releases obtained from the official
> Cornell-LPS repository.

## Running the Application (source files)

1. Requirements:

- Windows
- Visual Studio with .NET desktop development support
- Python with the required scientific packages
- FastAPI and Uvicorn

2. Clone the repository and open `ISCRA_app.slnx` in Visual Studio

```powershell
git clone https://github.com/Cornell-LPS/ISCRA_app.git
cd ISCRA_app
```

## Workflow

1. Enter the shot number and select an output directory.
2. Open the calibration-lamp `.sif` file.
3. Find the lamp peaks.
4. Open the experimental OES `.sif` file.
5. Auto-detect or import the fiber boxes.
6. Begin calibration and assign wavelengths to selected lamp peaks.
7. Inspect calibrated spectra.
8. Save the desired analysis outputs.

## Reference-Line CSV Files

Reference spectral-line records are not distributed with this repository or
its releases. Header-only templates are provided for:

```text
dataNeon.csv
dataKrypton.csv
dataMercury.csv
dataOxygen.csv
dataHydrogen.csv
```

Each file must use this header:

```csv
element,sp_num,ritz_wl_air(nm),intens,J_i,J_k
```

The wavelength must be given numerically in nanometers. Other fields may be
left blank when unavailable.

## Data Source

The application is designed to use reference data obtained from the:

**NIST Atomic Spectra Database, Standard Reference Database 78, Version 5.12**

Users must obtain any required data directly from NIST and comply with the
applicable copyright and licensing terms.

Citation:

> Kramida, A., Ralchenko, Yu., Reader, J., and NIST ASD Team,  
> NIST Atomic Spectra Database, Version 5.12,  
> NIST Standard Reference Database 78.  
> DOI: 10.18434/T4W30F.

NIST has not endorsed this application.

## Project Files

- `WindowInit.cs` - Windows Forms interface
- `api_connection_file.py` - FastAPI backend
- `calibrate.py` - calibration and spectrum processing
- `oes_io.py` - file loading and data input/output
- `plot.py` - plotting functions

## Acknowledgments

Developed for the Cornell Laboratory of Plasma Studies under the supervision of Professor Jack D. Hare, and with support from Luke Filor.
