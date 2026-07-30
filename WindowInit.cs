using Microsoft.VisualBasic;
using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Security.Policy;
using System.Text;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;



public class WindowInit : Form
{
    //public enum MessageBoxResult;

    private string appfolder = AppDomain.CurrentDomain.BaseDirectory;
    private string neon_path;
    private string shot_path;
    private string output_path;
    private string fiber_path;
    private readonly HttpClient client = new HttpClient();
    private Process backendProcess;
    private System.Windows.Forms.Button buttonDone;
    private TaskCompletionSource<bool> doneClicked;
    private bool done;
    private List<FiberBox> fiber_boxes;
    private List<PeakPoint> peak_arr_loc = new List<PeakPoint>();
    private List<float> neon_wave = new List<float>();
    private List<(int, int)> y_regions = new List<(int, int)>();
    private List<float> peaks_pressed = new List<float>();
    private PictureBox plotPictureBox;
    private Label informationLabel;
    private int neon_image_width;
    private bool calibrated = false;
    private float center_wavelength;
    private int shot_number;
    //private ScriptScope scope;
    //private ScriptEngine engine;
    private TableLayoutPanel outerPanel;
    private TableLayoutPanel innerPanel;
    private TableLayoutPanel innerPanel1;
    private TableLayoutPanel radioPanel;
    //private TabPage tabPage1;
    //private TabPage tabPage2;
    //private TabControl tabControl1;
    private MenuStrip mainMenu;
    private ToolStripMenuItem editMenu;
    private ToolStripMenuItem processMenu;
    private ToolStripMenuItem fileMenu;
    private ToolStripMenuItem helpMenu;
    private ToolStripMenuItem helpDataSources;

    private ToolStripMenuItem editOpenNeon;
    private ToolStripMenuItem editOpenOes;
    private ToolStripMenuItem editOpenOutput;
    private ToolStripMenuItem editShotNumber;

    private ToolStripMenuItem fileSaveFibers;
    private ToolStripMenuItem fileSaveLocations;
    private ToolStripMenuItem fileSaveEnergies;

    private ToolStripMenuItem processFindNeon;
    private ToolStripMenuItem processFibers;
    private ToolStripMenuItem processImportFibers;
    private ToolStripMenuItem processAutoFibers;
    private ToolStripMenuItem processCalibrate;

    private RadioButton radioNeon;
    private RadioButton radioFibersBoxes;
    private RadioButton radioFibersPlot;
    private RadioButton radioMedianFibers;
    private RadioButton radioWavelengthPixel;

    private System.Windows.Forms.TextBox boxShotNumber;
    private DataGridView neonSpec;
    private System.Windows.Forms.ComboBox elementSelection;
    private System.Windows.Forms.TextBox wavelengthFrom;
    private System.Windows.Forms.TextBox wavelengthTo;
    private bool peaksFound = false;

    private readonly Dictionary<string, string> spectrumFiles =
        new Dictionary<string, string>
        {
        { "Ne", "dataNeon.csv" },
        { "Kr", "dataKrypton.csv" },
        { "Hg", "dataMercury.csv" },
        { "O", "dataOxygen.csv" },
        { "H", "dataHydrogen.csv" }
        };


    public WindowInit()
    {
        MainWindow();
        RadioButtons();
        MenuControl();
        Console.WriteLine(AppDomain.CurrentDomain.BaseDirectory);
        neon_spectrum();
        KeyPresses();
        this.Shown += WindowInit_Shown;
        this.FormClosed += WindowInit_FormClosed;

    }

    private void MenuControl()
    {

        this.mainMenu = new MenuStrip();
        this.editMenu = new ToolStripMenuItem();
        this.processMenu = new ToolStripMenuItem();
        this.fileMenu = new ToolStripMenuItem();
        this.helpMenu = new ToolStripMenuItem();
        this.helpDataSources = new ToolStripMenuItem();

        this.editOpenNeon = new ToolStripMenuItem();
        this.editOpenOes = new ToolStripMenuItem();
        this.editOpenOutput = new ToolStripMenuItem();
        this.editShotNumber = new ToolStripMenuItem();

        this.fileSaveFibers = new ToolStripMenuItem();
        this.fileSaveEnergies = new ToolStripMenuItem();
        this.fileSaveLocations = new ToolStripMenuItem();

        this.processFindNeon = new ToolStripMenuItem();
        this.processFibers = new ToolStripMenuItem();
        this.processImportFibers = new ToolStripMenuItem();
        this.processAutoFibers = new ToolStripMenuItem();
        this.processCalibrate = new ToolStripMenuItem();


        editMenu.Text = "Edit";
        processMenu.Text = "Process";
        fileMenu.Text = "File";
        helpMenu.Text = "Help";
        helpDataSources.Text = "Data Sources";

        this.helpDataSources.ForeColor = Color.FromArgb(71, 79, 92);
        this.helpDataSources.BackColor = Color.FromArgb(193, 214, 247);
        this.editMenu.ForeColor = Color.FromArgb(71, 79, 92);
        this.editMenu.BackColor = Color.FromArgb(193, 214, 247);
        this.processMenu.ForeColor = Color.FromArgb(71, 79, 92);
        this.processMenu.BackColor = Color.FromArgb(193, 214, 247);
        this.fileMenu.ForeColor = Color.FromArgb(71, 79, 92);
        this.fileMenu.BackColor = Color.FromArgb(193, 214, 247);
        this.helpMenu.ForeColor = Color.FromArgb(71, 79, 92);
        this.helpMenu.BackColor = Color.FromArgb(193, 214, 247);


        //file menu:

        fileSaveFibers.Text = "Save Fiber Spectrum";
        fileSaveEnergies.Text = "Save Photon Energy and Intensity Columns";
        fileSaveLocations.Text = "Save Fiber Box Coordinates";

        this.fileSaveFibers.BackColor = Color.FromArgb(193, 214, 247);
        this.fileSaveFibers.ForeColor = Color.FromArgb(71, 79, 92);
        this.fileSaveEnergies.BackColor = Color.FromArgb(193, 214, 247);
        this.fileSaveEnergies.ForeColor = Color.FromArgb(71, 79, 92);
        this.fileSaveLocations.BackColor = Color.FromArgb(193, 214, 247);
        this.fileSaveLocations.ForeColor = Color.FromArgb(71, 79, 92);

        //edit menu:

        editOpenNeon.Text = "Open a Lamp Spectrum File";
        editOpenOes.Text = "Open an OES File ";
        editOpenOutput.Text = "Select an Output Directory";
        editShotNumber.Text = "Re-enter Shot Number";

        this.editOpenNeon.BackColor = Color.FromArgb(193, 214, 247);
        this.editOpenNeon.ForeColor = Color.FromArgb(71, 79, 92);
        this.editOpenOes.BackColor = Color.FromArgb(193, 214, 247);
        this.editOpenOes.ForeColor = Color.FromArgb(71, 79, 92);
        this.editOpenOutput.BackColor = Color.FromArgb(193, 214, 247);
        this.editOpenOutput.ForeColor = Color.FromArgb(71, 79, 92);
        this.editShotNumber.BackColor = Color.FromArgb(193, 214, 247);
        this.editShotNumber.ForeColor = Color.FromArgb(71, 79, 92);


        //process menu:
        processFindNeon.Text = "Find Neon Peaks";
        processFibers.Text = "Process Fiber-optics";
        processAutoFibers.Text = "Auto-find Fiber Data Boxes";
        processImportFibers.Text = "Import Fiber Data Box Coordinates";
        processCalibrate.Text = "Calibrate Data";

        this.processFindNeon.BackColor = Color.FromArgb(193, 214, 247);
        this.processFindNeon.ForeColor = Color.FromArgb(71, 79, 92);
        this.processFibers.BackColor = Color.FromArgb(193, 214, 247);
        this.processFibers.ForeColor = Color.FromArgb(71, 79, 92);
        this.processAutoFibers.BackColor = Color.FromArgb(193, 214, 247);
        this.processAutoFibers.ForeColor = Color.FromArgb(71, 79, 92);
        this.processImportFibers.BackColor = Color.FromArgb(193, 214, 247);
        this.processImportFibers.ForeColor = Color.FromArgb(71, 79, 92);
        this.processCalibrate.BackColor = Color.FromArgb(193, 214, 247);
        this.processCalibrate.ForeColor = Color.FromArgb(71, 79, 92);

        this.Controls.Add(mainMenu);
        //this.mainMenu.BringToFront();

        mainMenu.BackColor = Color.FromArgb(193, 214, 247);
        mainMenu.Items.Add(this.fileMenu);
        mainMenu.Items.Add(this.editMenu);
        mainMenu.Items.Add(this.processMenu);
        mainMenu.Items.Add(this.helpMenu);
        helpMenu.DropDownItems.Add(helpDataSources);
        fileMenu.DropDownItems.Add(fileSaveFibers);
        fileMenu.DropDownItems.Add(fileSaveLocations);
        fileMenu.DropDownItems.Add(fileSaveEnergies);
        processMenu.DropDownItems.Add(processFibers);
        processFibers.DropDownItems.Add(processAutoFibers);
        processFibers.DropDownItems.Add(processImportFibers);
        processMenu.DropDownItems.Add(processCalibrate);
        processMenu.DropDownItems.Add(processFindNeon);
        editMenu.DropDownItems.Add(editOpenNeon);
        editMenu.DropDownItems.Add(editOpenOes);
        editMenu.DropDownItems.Add(editOpenOutput);
        editMenu.DropDownItems.Add(editShotNumber);
    }
    private void RadioButtons()

    {
        this.radioNeon = new RadioButton();
        this.radioFibersBoxes = new RadioButton();
        this.radioFibersPlot = new RadioButton();
        this.radioMedianFibers = new RadioButton();
        this.radioWavelengthPixel = new RadioButton();
        this.boxShotNumber = new System.Windows.Forms.TextBox();

        radioNeon.Text = "Neon Spectrum";
        radioFibersBoxes.Text = "Fiber Boxes";
        radioFibersPlot.Text = "Fibers Plot";
        radioMedianFibers.Text = "Median of Fibers";
        radioWavelengthPixel.Text = "Wavelength vs Pixel";
        boxShotNumber.Text = "Shot Number";


        this.radioNeon.ForeColor = Color.FromArgb(71, 79, 92);
        this.radioFibersBoxes.ForeColor = Color.FromArgb(71, 79, 92);
        this.radioFibersPlot.ForeColor = Color.FromArgb(71, 79, 92);
        this.radioMedianFibers.ForeColor = Color.FromArgb(71, 79, 92);
        this.radioWavelengthPixel.ForeColor = Color.FromArgb(71, 79, 92);
        this.boxShotNumber.ForeColor = Color.FromArgb(71, 79, 92);


        radioNeon.Width = 150;
        radioFibersBoxes.Width = 150;
        radioFibersPlot.Width = 150;
        radioMedianFibers.Width = 150;
        radioWavelengthPixel.Width = 150;
        radioNeon.Height = 30;
        radioFibersBoxes.Height = 30;
        radioFibersPlot.Height = 30;
        radioMedianFibers.Height = 30;
        radioWavelengthPixel.Height = 30;


        this.radioNeon.Dock = DockStyle.Top;
        this.radioFibersBoxes.Dock = DockStyle.Top;
        this.radioFibersPlot.Dock = DockStyle.Top;
        this.radioMedianFibers.Dock = DockStyle.Top;
        this.radioWavelengthPixel.Dock = DockStyle.Top;
        //this.boxShotNumber.Dock = DockStyle.Left;
        boxShotNumber.Width = 130;
        boxShotNumber.Height = 30;
        this.boxShotNumber.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        boxShotNumber.Padding = new Padding(20, 0, 0, 0);

        radioPanel.Controls.Add(radioNeon);
        radioPanel.Controls.Add(radioFibersBoxes);
        radioPanel.Controls.Add(radioFibersPlot);
        radioPanel.Controls.Add(radioMedianFibers);
        radioPanel.Controls.Add(radioWavelengthPixel);
        innerPanel1.Controls.Add(boxShotNumber, 0, 0);

    }

    private void MainWindow()
    {
        this.radioPanel = new TableLayoutPanel();


        this.outerPanel = new TableLayoutPanel();
        this.ClientSize = new Size(1500, 1000);
        this.Text = "OES Analysis Tool";

        this.outerPanel.Dock = DockStyle.Fill;
        this.outerPanel.BackColor = Color.FromArgb(193, 214, 247);
        this.outerPanel.ColumnCount = 2;
        this.outerPanel.RowCount = 1;

        this.outerPanel.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 75)
        );
        this.outerPanel.ColumnStyles.Add(
            new ColumnStyle(SizeType.Percent, 25)
        );

        this.outerPanel.CellBorderStyle =
            TableLayoutPanelCellBorderStyle.None;

        this.innerPanel = new TableLayoutPanel();

        this.innerPanel.Dock = DockStyle.Fill;
        this.innerPanel.BackColor = Color.FromArgb(193, 214, 247);
        this.innerPanel.ColumnCount = 1;
        this.innerPanel.RowCount = 2;

        this.innerPanel.RowStyles.Add(
            new RowStyle(SizeType.Percent, 30)
        );
        this.innerPanel.RowStyles.Add(
            new RowStyle(SizeType.Percent, 70)
        );

        this.innerPanel.CellBorderStyle =
            TableLayoutPanelCellBorderStyle.None;

        this.innerPanel1 = new TableLayoutPanel();

        this.innerPanel1.Dock = DockStyle.Fill;
        this.innerPanel1.BackColor = Color.FromArgb(193, 214, 247);
        this.innerPanel1.ColumnCount = 1;
        this.innerPanel1.RowCount = 3;

        this.innerPanel1.RowStyles.Add(
            new RowStyle(SizeType.Percent, 5)
        );
        this.innerPanel1.RowStyles.Add(
            new RowStyle(SizeType.Percent, 75)
        );
        this.innerPanel1.RowStyles.Add(
            new RowStyle(SizeType.Percent, 20)
        );

        this.innerPanel1.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
        this.innerPanel1.CellPaint += (sender, e) => CustomCellPaint(e, 255, 255, 255);
        this.innerPanel.CellPaint += (sender, e) => CustomCellPaint(e, 255, 255, 255);
        this.outerPanel.CellPaint += (sender, e) => CustomCellPaint(e, 255, 255, 255);
        //this.tabPage1 = new TabPage();     
        //this.tabControl1 = new TabControl();
        //tabPage1.Text = "Lamp Spectrum";
        //tabPage1.Size = new Size(256, 214);
        //tabPage1.TabIndex = 0;
        //this.tabControl1.Dock = DockStyle.Fill;
        //this.tabPage1.BackColor = Color.FromArgb(38, 45, 56);

        //this.tabPage2 = new TabPage();
        //tabPage2.Text = "Fiber-optic data";
        //tabPage2.Size = new Size(256, 214);
        //tabPage2.TabIndex = 1;
        //this.tabControl1.Dock = DockStyle.Fill;
        //this.tabPage2.BackColor = Color.FromArgb(38, 45, 56);
        this.radioPanel.Dock = DockStyle.Fill;
        this.radioPanel.BackColor = Color.FromArgb(193, 214, 247);

        ////radioPanel.AutoSize = true;
        //innerPanel.ColumnStyles[0].SizeType = SizeType.Absolute;
        //innerPanel.ColumnStyles[0].Width = 200F;


        outerPanel.Controls.Add(innerPanel, 1, 0);
        innerPanel.Controls.Add(radioPanel);
        outerPanel.Controls.Add(innerPanel1, 0, 0);
        this.Controls.Add(outerPanel);

        this.plotPictureBox = new PictureBox();
        this.plotPictureBox.Dock = DockStyle.Fill;
        this.plotPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
        this.innerPanel1.Controls.Add(plotPictureBox, 0, 1);

        this.informationLabel = new Label();
        this.informationLabel.Dock = DockStyle.Fill;
        this.informationLabel.ForeColor = Color.FromArgb(71, 79, 92);
        this.informationLabel.Padding = new Padding(10);
        this.innerPanel1.Controls.Add(informationLabel, 0, 2);

        //tabControl1.Controls.Add(this.tabPage1);
        //tabControl1.Controls.Add(this.tabPage2);
        //this.innerPanel1.Controls.Add(this.tabControl1);
        //this.innerPanel1.Controls.Add(this.tabControl1);
    }
    private void CustomCellPaint(TableLayoutCellPaintEventArgs e, int red, int green, int blue)
    {
        using (Pen customPen = new Pen(Color.FromArgb(red, green, blue), 1))
        {
            e.Graphics.DrawRectangle(customPen, e.CellBounds);
        }
    }
    private void KeyPresses()
    {
        editOpenOes.Click += editOpenOes_Click;
        editOpenNeon.Click += editOpenNeon_Click;
        editOpenOutput.Click += editOpenOutput_Click;
        editShotNumber.Click += editShotNumber_Click;

        processFindNeon.Click += processFindNeon_Click;
        processAutoFibers.Click += processAutoFibers_Click;
        processImportFibers.Click += processImportFibers_Click;
        processCalibrate.Click += processCalibrate_Click;

        fileSaveEnergies.Click += fileSaveEnergies_Click;
        fileSaveFibers.Click += fileSaveFibers_Click;
        fileSaveLocations.Click += fileSaveLocations_Click;
        
        helpDataSources.Click += helpDataSources_Click;

        radioNeon.Click += radioNeon_Click;
        radioFibersBoxes.Click += radioFibersBoxes_Click;
        radioFibersPlot.Click += radioFibersPlot_Click;
        radioMedianFibers.Click += radioMedianFibers_Click;
        radioWavelengthPixel.Click += radioWavelengthPixel_Click;
        boxShotNumber.KeyDown += boxShotNumber_KeyDown;

    }

    private void GetPythonPeaks()
    {
        if (File.Exists("peak_arr_loc.json"))
        {
            string jsonString = File.ReadAllText("peak_arr_loc.json");
            peak_arr_loc = JsonSerializer.Deserialize<List<PeakPoint>>(jsonString);
        }

        else
        {
            MessageBox.Show("The algorithm does not recognize any peaks.");
        }


    }
    private void helpDataSources_Click(object sender, EventArgs e)
    {
        MessageBox.Show(
            "Reference spectral-line data are not included in this repository.\n\n" +
            "The application uses data obtained from the NIST Atomic Spectra " +
            "Database, NIST Standard Reference Database 78, Version 5.12.\n\n" +
            "Users must obtain any required data directly from NIST and comply " +
            "with NIST's applicable copyright and licensing terms.",
            "Data Availability",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );
    }
    private async void PixelPressesDynamic(object sender, MouseEventArgs e)
    {
        if (plotPictureBox.Image == null || peak_arr_loc.Count == 0)
        {
            return;
        }

        float imgX = (float)e.X * plotPictureBox.Image.Width / plotPictureBox.Width;

        PeakPoint closestpeak = peak_arr_loc.OrderBy(peak => Math.Abs(peak.X - imgX)).First();

        if (Math.Abs(closestpeak.X - imgX) > 40)
        {
            return;
        }

        string input = Interaction.InputBox($"Assign wavelength for Peak at X: {closestpeak.Pixel}", "Wavelength Input");

        if (float.TryParse(input, out float wvlngth))
        {
            var openRequest = new { clicked_pixel = closestpeak.Pixel, clicked_neon = wvlngth, done = false };
            var response = await client.PostAsJsonAsync("http://127.0.0.1:8000/lamp-emission-lines", openRequest);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ProcessFindNeon_Response>();
            //MessageBox.Show(result.message);
        }
    }


    private async void radioNeon_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(neon_path))
        {
            MessageBox.Show("Open a lamp emission file first.");
            return;
        }

        var openRequest = new { };
        var response = await client.PostAsJsonAsync(calibrated ? "http://127.0.0.1:8000/plot-neon-updated" : "http://127.0.0.1:8000/plot-neon", openRequest);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ProcessFindNeon_Response>();
        peak_arr_loc = result.peak_arr_loc;
        ImageShow(result.image);

        if (buttonDone != null)
        {
            plotPictureBox.MouseClick -= PixelPressesDynamic;
            plotPictureBox.MouseClick += PixelPressesDynamic;
            informationLabel.Text = "Please assign wavelengths to emission lines of choice by clicking on the peak and entering the wavelength.";
        }
        else
        {
            plotPictureBox.MouseClick -= PixelPressesDynamic;
            informationLabel.Text = calibrated ? $"Center wavelength: {center_wavelength} nm" : "";
        }
    }

    private async void radioFibersBoxes_Click(object sender, EventArgs e)
    {
        if (fiber_boxes == null || !fiber_boxes.Any())
        {
            MessageBox.Show("Upload or auto-find the fiber boxes first.");
            return;
        }

        var openRequest = new { };
        var response = await client.PostAsJsonAsync("http://127.0.0.1:8000/show-fiber-boxes", openRequest);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PlotResponse>();
        ImageShow(result.image);
        plotPictureBox.MouseClick -= PixelPressesDynamic;
        informationLabel.Text = string.Join(Environment.NewLine, fiber_boxes.Select((box, i) => $"Fiber {i + 1}: x0={box.x0}, y0={box.y0}, width={box.width}, height={box.height}"));
    }

    private async void radioFibersPlot_Click(object sender, EventArgs e)
    {
        if (!calibrated)
        {
            MessageBox.Show("Perform the calibration of the data first.");
            return;
        }

        var openRequest = new { shot_number = 0 };
        var response = await client.PostAsJsonAsync("http://127.0.0.1:8000/show-fibers-plot", openRequest);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<PlotResponse>();
        ImageShow(result.image);
        plotPictureBox.MouseClick -= PixelPressesDynamic;
        informationLabel.Text = $"Number of fibers: {fiber_boxes.Count}{Environment.NewLine}Legend: Fiber 1 to Fiber {fiber_boxes.Count}";
    }

    private async void radioMedianFibers_Click(object sender, EventArgs e)
    {
        if (!calibrated)
        {
            MessageBox.Show("Perform the calibration of the data first.");
            return;
        }

        var openRequest = new { };
        var response = await client.PostAsJsonAsync("http://127.0.0.1:8000/show-median-fibers", openRequest);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<MedianFibers_Response>();
        ImageShow(result.image);
        plotPictureBox.MouseClick -= PixelPressesDynamic;
        informationLabel.Text = $"Peaks found: {string.Join(", ", result.peaks.Select(peak => $"{peak:F3} nm"))}";
    }

    private async void radioWavelengthPixel_Click(object sender, EventArgs e)
    {
        if (!calibrated)
        {
            MessageBox.Show("Perform the calibration of the data first.");
            return;
        }

        var openRequest = new { };
        var response = await client.PostAsJsonAsync("http://127.0.0.1:8000/show-wavelength-pixel", openRequest);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<WavelengthPixel_Response>();
        ImageShow(result.image);
        plotPictureBox.MouseClick -= PixelPressesDynamic;
        informationLabel.Text = result.equation;
    }



    private async void editOpenOes_Click(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(shot_path))
        {
            DialogResult result3 = MessageBox.Show(
            "You have already opened an OES spectrum. Opening another one will overwrite the existing information. Would you like to continue?",
            "Confirm",
            MessageBoxButtons.OKCancel,
            MessageBoxIcon.Warning
        );

            if (result3 == DialogResult.Cancel)
                return;
        }

        using OpenFileDialog dialog = new OpenFileDialog();
        dialog.Title = "Open an OES file";
        dialog.Filter = "SIF files (*.sif)|*.sif|All files (*.*)|*.*";

        if (dialog.ShowDialog() != DialogResult.OK) { return; }

        shot_path = dialog.FileName;

        var openRequest = new { shot_path = shot_path };

        //should you do the load_image and sif_to_csv here and for the neon as well?

        var response = await client.PostAsJsonAsync("http://127.0.0.1:8000/open-oes-file", openRequest);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<EditOpenOes_Response>();

        MessageBox.Show("OES spectrum successfully uploaded.");

    }

    private async void boxShotNumber_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            if (int.TryParse(boxShotNumber.Text, out int parsedShotNumber))
            {
                shot_number = parsedShotNumber;

                var openRequest = new { shot_number };

                var response = await client.PostAsJsonAsync(
                    "http://127.0.0.1:8000/shot-number",
                    openRequest
                );

                response.EnsureSuccessStatusCode();

                var result =
                    await response.Content.ReadFromJsonAsync<ShotNumber>();

                boxShotNumber.Enabled = false;
                MessageBox.Show(result.status);
            }
            else
            {
                MessageBox.Show("Please enter an integer.");
            }
        }
    }
    private void ImageShow(string base64Image)
    {
        byte[] imageBytes = Convert.FromBase64String(base64Image);

        using MemoryStream stream = new MemoryStream(imageBytes);
        using System.Drawing.Image temp = System.Drawing.Image.FromStream(stream);

        plotPictureBox.Image?.Dispose();
        plotPictureBox.Image = new Bitmap(temp);
    }
    private async void editOpenNeon_Click(object sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(neon_path))
        {
            DialogResult result3 = MessageBox.Show(
            "You have recently opened a lamp emission spectrum. Opening another one will overwrite any existing information. Would you like to continue?",
            "Confirm",
            MessageBoxButtons.OKCancel,
            MessageBoxIcon.Warning
        );

            if (result3 == DialogResult.Cancel)
                return;
        }
        using OpenFileDialog dialog = new OpenFileDialog();
        dialog.Title = "Open lamp emission spectrum file";
        dialog.Filter = "SIF files (*.sif)|*.sif|All files (*.*)|*.*";

        if (dialog.ShowDialog() != DialogResult.OK) { return; }

        neon_path = dialog.FileName;
        peaksFound = false;

        var openRequest = new { neon_path = neon_path };

        var response = await client.PostAsJsonAsync("http://127.0.0.1:8000/open-lamp-spectrum", openRequest);

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<EditOpenNeon_Response>();
        peak_arr_loc = result.peak_arr_loc;
        //radioNeon.Checked = true;


        //buttonDone.Click += processFindNeon();

        MessageBox.Show("Lamp spectrum successfully uploaded.");
    }
    private async void editOpenOutput_Click(object sender, EventArgs e)
    {
        
        if (!string.IsNullOrWhiteSpace(output_path))
        {
            DialogResult result3 = MessageBox.Show(
            "You have recently selected an output directory. Opening another one will overwrite any existing information. Would you like to continue?",
            "Confirm",
            MessageBoxButtons.OKCancel,
            MessageBoxIcon.Warning
        );

            if (result3 == DialogResult.Cancel)
                return;
        }

        using FolderBrowserDialog dialog = new FolderBrowserDialog();
        dialog.Description = "Select an Output Directory";
        if (dialog.ShowDialog() != DialogResult.OK) { return; }
        output_path = dialog.SelectedPath;
        MessageBox.Show("Output folder selected.");

    }

    private void editShotNumber_Click(object sender, EventArgs e)
    {
        DialogResult result = MessageBox.Show(
            "Caution: Editing the shot number now will not change the names of already saved files. Do you want to proceed?",
            "Confirm",
            MessageBoxButtons.OKCancel
        );

        if (result == DialogResult.OK)
        {
            boxShotNumber.Enabled = true;
        }
    }

    private async void processFindNeon_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(neon_path))
        {
            MessageBox.Show("Open a lamp emission file first.");
            return;
        }
        //peak_arr_loc = GetPythonPeaks();

        var openRequest = new { };
        var response = await client.PostAsJsonAsync("http://127.0.0.1:8000/plot-neon", openRequest);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ProcessFindNeon_Response>();

        peak_arr_loc = result.peak_arr_loc;
        peaksFound = peak_arr_loc != null && peak_arr_loc.Count > 0; radioNeon.Checked = true;
        radioNeon.Checked = true;
        ImageShow(result.image);

        plotPictureBox.MouseClick -= PixelPressesDynamic;
        informationLabel.Text = "";

        //MessageBox.Show(result.status);

    }
    private async void processAutoFibers_Click(object sender, EventArgs e)
    {
        if (shot_number <= 0)
        {
            MessageBox.Show("Need to enter shot number first.");
            return;
        }

        string UserInput = Interaction.InputBox("Please enter the number of optical fibers: ", "Number of fibers", "1");
        if (!int.TryParse(UserInput, out int n_fibers)) { return; }

        var openRequest = new { n_fibers = n_fibers };
        var response = await client.PostAsJsonAsync("http://127.0.0.1:8000/auto-find-fiber-box-locations", openRequest);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ProcessAutoFibers_Response>();
        fiber_boxes = result.fiber_boxes;
        radioFibersBoxes.Checked = true;
        ImageShow(result.image);
        plotPictureBox.MouseClick -= PixelPressesDynamic;
        informationLabel.Text = string.Join(Environment.NewLine, fiber_boxes.Select((box, i) => $"Fiber {i + 1}: x0={box.x0}, y0={box.y0}, width={box.width}, height={box.height}"));
        //MessageBox.Show("Fiber boxes");
    }
    private async void processImportFibers_Click(object sender, EventArgs e)
    {
        if (shot_number <= 0)
        {
            MessageBox.Show("Need to enter shot number first.");
            return;
        }

        string UserInput = Interaction.InputBox("Please enter the number of optical fibers: ", "Number of fibers", "1");

        if (!int.TryParse(UserInput, out int n_fibers)) { return; }
        using OpenFileDialog dialog = new OpenFileDialog();
        dialog.Title = "Open fiber coordinate file";
        dialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";

        if (dialog.ShowDialog() != DialogResult.OK) { return; }
        fiber_path = dialog.FileName;
        var openRequest = new { fiber_path = fiber_path, expected_n_fibers = n_fibers };
        var response = await client.PostAsJsonAsync("http://127.0.0.1:8000/upload-fiber-box-coordinates", openRequest);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ProcessImportFibers_Response>();
        fiber_boxes = result.fiber_boxes;
        radioFibersBoxes.Checked = true;
        ImageShow(result.image);
        plotPictureBox.MouseClick -= PixelPressesDynamic;
        informationLabel.Text = string.Join(Environment.NewLine, fiber_boxes.Select((box, i) => $"Fiber {i + 1}: x0={box.x0}, y0={box.y0}, width={box.width}, height={box.height}"));
        //MessageBox.Show(result.status);
    }

    private async void processCalibrate_Click(object sender, EventArgs e)
    {

        if (!peaksFound)
        {
            MessageBox.Show("Find lamp peaks first.");
            return;
        }
        if (string.IsNullOrWhiteSpace(shot_path))
        {
            MessageBox.Show("Open an OES file first.");
            return;
        }

        if (string.IsNullOrWhiteSpace(fiber_path) && (fiber_boxes == null || !fiber_boxes.Any()))
        {
            MessageBox.Show("Upload or auto-find optical fibers spectrum first.");
            return;
        }

        if (string.IsNullOrWhiteSpace(neon_path))
        {
            MessageBox.Show("Open and process a lamp emission spectrum first.");
            return;
        }

        if (string.IsNullOrWhiteSpace(output_path))
        {
            MessageBox.Show("Please select an output directory first.");
            return;
        }

        var neonReq = new { };
        var neonRes = await client.PostAsJsonAsync("http://127.0.0.1:8000/plot-neon", neonReq);
        neonRes.EnsureSuccessStatusCode();
        var neon = await neonRes.Content.ReadFromJsonAsync<ProcessFindNeon_Response>();

        peak_arr_loc = neon.peak_arr_loc;
        radioNeon.Checked = true;
        ImageShow(neon.image);
        plotPictureBox.MouseClick -= PixelPressesDynamic;
        plotPictureBox.MouseClick += PixelPressesDynamic;
        MessageBox.Show("Please assign wavelengths to emission lines of choice by clicking on the peak and entering the wavelength.");

        doneClicked = new TaskCompletionSource<bool>();

        innerPanel1.Controls.Remove(informationLabel);
        buttonDone = new System.Windows.Forms.Button();
        buttonDone.BackColor = Color.FromArgb(71, 79, 92);
        buttonDone.ForeColor = Color.FromArgb(193, 214, 247);
        buttonDone.Text = "Done";
        buttonDone.Dock = DockStyle.Fill;
        buttonDone.Click += processButtonDone_Click;
        innerPanel1.Controls.Add(buttonDone, 0, 2);

        await doneClicked.Task;

        var doneReq = new { clicked_pixel = (float?)null, clicked_neon = (float?)null, done = true };
        var doneRes = await client.PostAsJsonAsync("http://127.0.0.1:8000/lamp-emission-lines", doneReq);
        doneRes.EnsureSuccessStatusCode();
        var doneResult = await doneRes.Content.ReadFromJsonAsync<ProcessFindNeon_Response>();
        peaks_pressed = doneResult.peaks_pressed;
        neon_wave = doneResult.neon_wave;

        var openReq = new { output_path = output_path };
        var openRes = await client.PostAsJsonAsync("http://127.0.0.1:8000/calibrate", openReq);
        openRes.EnsureSuccessStatusCode();

        var final = await openRes.Content.ReadFromJsonAsync<ProcessCalibrate_Response>();

        calibrated = true;
        center_wavelength = final.center_wavelength;

        var updatedReq = new { };
        var updatedRes = await client.PostAsJsonAsync("http://127.0.0.1:8000/plot-neon-updated", updatedReq);
        updatedRes.EnsureSuccessStatusCode();
        var updated = await updatedRes.Content.ReadFromJsonAsync<ProcessFindNeon_Response>();

        radioNeon.Checked = true;
        ImageShow(updated.image);
        plotPictureBox.MouseClick -= PixelPressesDynamic;
        informationLabel.Text = $"Center wavelength: {center_wavelength} nm";

        await Task.Delay(5000);

        var plotReq = new { shot_number = 0 };
        var plotRes = await client.PostAsJsonAsync("http://127.0.0.1:8000/show-fibers-plot", plotReq);
        plotRes.EnsureSuccessStatusCode();

        var plot = await plotRes.Content.ReadFromJsonAsync<PlotResponse>();

        radioFibersPlot.Checked = true;
        ImageShow(plot.image);
        informationLabel.Text = $"Number of fibers: {fiber_boxes.Count}{Environment.NewLine}Legend: Fiber 1 to Fiber {fiber_boxes.Count}";
        //MessageBox.Show(final.message);
    }

    private void processButtonDone_Click(object sender, EventArgs e)
    {
        innerPanel1.Controls.Remove(buttonDone);
        buttonDone.Dispose();
        buttonDone = null;
        innerPanel1.Controls.Add(informationLabel, 0, 2);
        doneClicked.TrySetResult(true);
    }

    private async void fileSaveEnergies_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(output_path))
        {
            MessageBox.Show("Please select an output directory.");
            return;
        }
        var openRequest = new { output_path = output_path };
        var response = await client.PostAsJsonAsync("http://127.0.0.1:8000/save-photon-energy", openRequest);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<FileSaveEnergies_Response>();
        //MessageBox.Show(result.status);
    }

    private async void fileSaveFibers_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(output_path))
        {
            MessageBox.Show("Please select an output directory.");
            return;
        }
        var openRequest = new { output_path = output_path };
        var response = await client.PostAsJsonAsync("http://127.0.0.1:8000/save-fiber-spectrum", openRequest);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<FileSaveFibers_Response>();
        //MessageBox.Show(result.status);
    }

    private async void fileSaveLocations_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(output_path))
        {
            MessageBox.Show("Please select an output directory.");
            return;
        }
        var openRequest = new { output_path = output_path };
        var response = await client.PostAsJsonAsync("http://127.0.0.1:8000/save-fiber-box-coordinates", openRequest);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<FileSaveLocations_Response>();
        //MessageBox.Show(result.status);
    }

    private bool StartBackend()
    {
        string appDirectory = AppContext.BaseDirectory;
        string pythonPath = Path.Combine(appDirectory, "python", "python.exe");
        string backendPath = Path.Combine(appDirectory, "backend");

        if (!File.Exists(pythonPath))
        {
            MessageBox.Show("The packaged Python executable could not be found.");
            return false;
        }

        if (!Directory.Exists(backendPath))
        {
            MessageBox.Show("The backend directory could not be found.");
            return false;
        }

        ProcessStartInfo backendStartInfo = new ProcessStartInfo();
        backendStartInfo.FileName = pythonPath;
        backendStartInfo.Arguments = "-m uvicorn api_connection_file:app --host 127.0.0.1 --port 8000";
        backendStartInfo.WorkingDirectory = backendPath;
        backendStartInfo.UseShellExecute = false;
        backendStartInfo.CreateNoWindow = true;

        backendProcess = new Process();
        backendProcess.StartInfo = backendStartInfo;
        backendProcess.Start();

        return true;
    }
    private async Task<bool> WaitForBackend()
    {
        for (int attempt = 0; attempt < 40; attempt++)
        {
            if (backendProcess == null || backendProcess.HasExited)
            {
                return false;
            }

            try
            {
                var response = await client.GetAsync("http://127.0.0.1:8000/health");

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch
            {

            }

            await Task.Delay(250);
        }

        return false;
    }
    private async void WindowInit_Shown(object sender, EventArgs e)
    {
        this.Enabled = false;

        if (!StartBackend())
        {
            this.Close();
            return;
        }

        bool backendReady = await WaitForBackend();

        if (!backendReady)
        {
            MessageBox.Show("The Python backend failed to start.");
            this.Close();
            return;
        }

        this.Enabled = true;
    }
    private void WindowInit_FormClosed(object sender, FormClosedEventArgs e)
    {
        try
        {
            if (backendProcess != null && !backendProcess.HasExited)
            {
                backendProcess.Kill();
                backendProcess.WaitForExit();
            }

            backendProcess?.Dispose();
        }
        catch
        {

        }
    }

    private void neon_spectrum()
    {
        this.neonSpec = new DataGridView();
        this.elementSelection = new System.Windows.Forms.ComboBox();
        this.wavelengthFrom = new System.Windows.Forms.TextBox();
        this.wavelengthTo = new System.Windows.Forms.TextBox();

        TableLayoutPanel spectrumPanel = new TableLayoutPanel();
        FlowLayoutPanel filterPanel = new FlowLayoutPanel();

        spectrumPanel.Dock = DockStyle.Fill;
        spectrumPanel.ColumnCount = 1;
        spectrumPanel.RowCount = 2;
        spectrumPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        spectrumPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        filterPanel.Dock = DockStyle.Fill;
        filterPanel.WrapContents = false;
        filterPanel.Padding = new Padding(5, 4, 0, 0);

        elementSelection.DropDownStyle = ComboBoxStyle.DropDownList;
        elementSelection.Width = 65;
        elementSelection.Items.AddRange(
            new object[] { "Ne", "Kr", "Hg", "O", "H" }
        );
        elementSelection.SelectedIndex = 0;

        wavelengthFrom.Width = 55;
        wavelengthTo.Width = 55;

        neonSpec.AllowUserToAddRows = false;
        neonSpec.AllowUserToDeleteRows = false;
        neonSpec.AllowUserToOrderColumns = false;
        neonSpec.Dock = DockStyle.Fill;

        elementSelection.SelectedIndexChanged +=
            (sender, e) => RefreshSpectrumTable();

        wavelengthFrom.Leave +=
            (sender, e) => RefreshSpectrumTable();

        wavelengthTo.Leave +=
            (sender, e) => RefreshSpectrumTable();

        wavelengthFrom.KeyDown += wavelengthRange_KeyDown;
        wavelengthTo.KeyDown += wavelengthRange_KeyDown;

        filterPanel.Controls.Add(elementSelection);
        filterPanel.Controls.Add(new Label
        {
            Text = "From",
            AutoSize = true,
            Margin = new Padding(5, 5, 3, 0)
        });
        filterPanel.Controls.Add(wavelengthFrom);
        filterPanel.Controls.Add(new Label
        {
            Text = "nm to",
            AutoSize = true,
            Margin = new Padding(3, 5, 3, 0)
        });
        filterPanel.Controls.Add(wavelengthTo);
        filterPanel.Controls.Add(new Label
        {
            Text = "nm",
            AutoSize = true,
            Margin = new Padding(3, 5, 3, 0)
        });

        spectrumPanel.Controls.Add(filterPanel, 0, 0);
        spectrumPanel.Controls.Add(neonSpec, 0, 1);

        innerPanel.Controls.Add(spectrumPanel, 0, 1);

        RefreshSpectrumTable();
    }

    private void wavelengthRange_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
            return;

        RefreshSpectrumTable();
        e.SuppressKeyPress = true;
    }

    private void RefreshSpectrumTable()
    {
        double minimumWavelength = 200;
        double maximumWavelength = 800;

        if (double.TryParse(wavelengthFrom.Text, out double enteredMinimum) &&
            double.TryParse(wavelengthTo.Text, out double enteredMaximum) &&
            enteredMinimum >= 200 &&
            enteredMaximum <= 800 &&
            enteredMinimum <= enteredMaximum
        )
        {
            minimumWavelength = enteredMinimum;
            maximumWavelength = enteredMaximum;
        }

        string selectedElement = elementSelection.SelectedItem.ToString();
        string fileName = spectrumFiles[selectedElement];

        LoadCsv(
            Path.Combine(appfolder, fileName),
            neonSpec,
            minimumWavelength,
            maximumWavelength
        );
    }

    private void LoadCsv(
        string filePath,
        DataGridView spectrum,
        double minimumWavelength = 200,
        double maximumWavelength = 800)
    {
        string[] selectedColumns =
        {
        "element",
        "sp_num",
        "ritz_wl_air(nm)",
        "intens",
        "J_i",
        "J_k"
    };

        DataTable dt = new DataTable();

        foreach (string column in selectedColumns)
            dt.Columns.Add(column);

        try
        {
            using (TextFieldParser parser = new TextFieldParser(filePath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = true;

                if (parser.EndOfData)
                    throw new Exception("The file is empty.");

                string[] headers = parser.ReadFields();

                int[] selectedIndexes = selectedColumns
                    .Select(column => Array.FindIndex(
                        headers,
                        header => string.Equals(
                            header.Trim(),
                            column,
                            StringComparison.OrdinalIgnoreCase)))
                    .ToArray();

                for (int i = 0; i < selectedIndexes.Length; i++)
                {
                    if (selectedIndexes[i] == -1)
                        throw new Exception(
                            $"Column '{selectedColumns[i]}' was not found.");
                }

                int wavelengthIndex = selectedIndexes[2];

                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();

                    if (fields == null || wavelengthIndex >= fields.Length)
                        continue;

                    if (!double.TryParse(
                        CleanCsvValue(fields[wavelengthIndex]),
                        out double wavelength))
                        continue;

                    if (wavelength < minimumWavelength ||
                        wavelength > maximumWavelength)
                        continue;

                    DataRow row = dt.NewRow();

                    for (int i = 0; i < selectedIndexes.Length; i++)
                    {
                        int sourceIndex = selectedIndexes[i];

                        row[i] = sourceIndex < fields.Length
                            ? CleanCsvValue(fields[sourceIndex])
                            : "";
                    }

                    dt.Rows.Add(row);
                }
            }

            spectrum.DataSource = dt;
            spectrum.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.AllCells;
        }
        catch (Exception ex)
        {
            spectrum.DataSource = dt;

            MessageBox.Show(
                $"Could not load the spectrum file:\n\n{ex.Message}",
                "File Loading Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }

    private string CleanCsvValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        value = value.Trim();

        if (value.StartsWith("=\"") && value.EndsWith("\""))
            return value.Substring(2, value.Length - 3);

        return value;
    }
}

public class EditOpenOes_Response
{
    public string status { get; set; }
    public string message { get; set; }
    public string shot_path { get; set; }
    public int[] shot_shape { get; set; }
}

public class EditOpenNeon_Response
{
    public string status { get; set; }
    public string message { get; set; }
    public string neon_path { get; set; }
    public int[] neon_shape { get; set; }
    public List<PeakPoint> peak_arr_loc { get; set; }
    //public string image { get; set; }
}

//public class EditOpenOutput_Response
//{
//    public string status { get; set; }
//    public string message { get; set; }
//    public string output_path { get; set; }
//}

public class ProcessCalibrate_Response
{
    public string status { get; set; }
    public string message { get; set; }
    //public int[] wavelength_nm { get; set; }
    //public string out_csv { get; set; }//unsure about this one
    public int center_col { get; set; }
    public float center_wavelength { get; set; }
    public string calibrated_csv { get; set; }
}

public class ProcessFindNeon_Response
{
    public string status { get; set; }
    public string message { get; set; }
    public List<float> peaks_pressed { get; set; }
    public List<float> neon_wave { get; set; }
    public List<PeakPoint> peak_arr_loc { get; set; }
    public List<float> peak_arr { get; set; }
    public string image { get; set; }
    public int neon_width { get; set; }
    public bool done { get; set; }
}

public class ProcessImportFibers_Response
{
    public string status { get; set; }
    public List<FiberBox> fiber_boxes { get; set; }
    public int x0 { get; set; }
    public int x1 { get; set; }
    public List<YRegion> y_regions { get; set; }
    public string image { get; set; }


}

public class ProcessAutoFibers_Response
{
    public string status { get; set; }
    public List<FiberBox> fiber_boxes { get; set; }
    public int x0 { get; set; }
    public int x1 { get; set; }
    public List<YRegion> y_regions { get; set; }
    public string image { get; set; }
}


public class MedianFibers_Response
{
    public string status { get; set; }
    public string shown { get; set; }
    public List<float> peaks { get; set; }
    public string image { get; set; }
}

public class WavelengthPixel_Response
{
    public string status { get; set; }
    public string shown { get; set; }
    public string equation { get; set; }
    public string image { get; set; }
}

public class PlotResponse
{
    public string status { get; set; }
    public string shown { get; set; }
    public string image { get; set; }
}

public class FileSaveFibers_Response
{
    public string status { get; set; }
    public string saved_spectrum_fibers { get; set; }

}

public class FileSaveEnergies_Response
{
    public string status { get; set; }
    public string photon_energy_file { get; set; }
}

public class FileSaveLocations_Response
{
    public string status { get; set; }
    public string fiber_coordinates { get; set; }
    public List<FiberBox> fiber_boxes { get; set; }
}

public class PeakPoint
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Pixel { get; set; }
}

public class FiberBox
{
    public int x0 { get; set; }
    public int y0 { get; set; }
    public int width { get; set; }
    public int height { get; set; }
    public int x1 { get; set; }
    public int y1 { get; set; }
}

public class YRegion
{
    public int y0 { get; set; }
    public int y1 { get; set; }
}

public class ProcessButtonDone_Response
{
    public string status { get; set; }
    public bool done { get; set; }
}

public class ShotNumber
{
    public string status { get; set; }
    public int shot_number { get; set; }
}

