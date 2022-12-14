﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Common_glTF_Exporter.ViewModel;
using Common_glTF_Exporter.Utils;
using System.IO;
using System.Threading;

namespace Revit_glTF_Exporter
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    
    public partial class Settings : Window
    {
        Document _doc;
        View3D _view;
        string _fileName;
        string _viewName;

        #if REVIT2019 || REVIT2020

        DisplayUnitType _internalProjectDisplayUnitType;
        DisplayUnitType _userDefinedDisplayUnitType;

        #else
        
        ForgeTypeId _internalProjectUnitTypeId;
        ForgeTypeId _userDefinedUnitTypeId;

        #endif

        UnitsViewModel _unitsViewModel;

        //public bool materialsExport = true;
        public Settings(Document doc, View3D view)
        {
            _unitsViewModel = new UnitsViewModel();
            this.DataContext = _unitsViewModel;

            InitializeComponent();

            #if REVIT2019 || REVIT2020

            _internalProjectDisplayUnitType = doc.GetUnits().GetFormatOptions(UnitType.UT_Length).DisplayUnits;
            UnitTextBlock.Content = LabelUtils.GetLabelFor(_internalProjectDisplayUnitType);

#else

            _internalProjectUnitTypeId = doc.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId();
            UnitTextBlock.Content = LabelUtils.GetLabelForUnit(_internalProjectUnitTypeId).ToString();

#endif

            UnitsComboBox.SelectedIndex = 0;

            this._doc = doc;
            this._view = view;
            this._fileName = _doc.Title;
            this._viewName = _view.Name;
        }

        private void OnExportView(object sender, RoutedEventArgs e)
        {
            if (_view == null)
            {
                TaskDialog.Show("glTFRevitExport", "You must be in a 3D view to export.");
                this.Close();
            }

            string fileName = string.Concat(_fileName, " - ", _viewName);
            bool dialogResult = FilesHelper.AskToSave(ref fileName, string.Empty, ".gltf");

            if (dialogResult == true)
            {
                string filename = fileName;
                string directory = System.IO.Path.GetDirectoryName(filename) + "\\";

                ExportView3D(_view, filename, directory, false);
            }
        }

        public void ExportView3D(View3D view3d, string filename, string directory, bool mode)
        {
            Document doc = view3d.Document;
            string directoryPath = Path.Combine(directory + "\\");

            ProgressBarWindow progressBar = new ProgressBarWindow();
            progressBar.ViewModel.ProgressBarValue = 0;
            progressBar.ViewModel.Message = "Converting elements...";
            progressBar.ViewModel.ProgressBarMax = Collectors.AllElementsByView(doc, doc.ActiveView).Count;
            progressBar.Show();
            ProgressBarWindow.MainView.Topmost = true;

            #if REVIT2019 || REVIT2020

            _userDefinedDisplayUnitType = _unitsViewModel.SelectedUnit.DisplayUnitType;

            // Use our custom implementation of IExportContext as the exporter context.
            glTFExportContext ctx = new glTFExportContext(doc, filename, directoryPath, _userDefinedDisplayUnitType, progressBar, true,
                true, FlipAxysCheckbox.IsChecked.Value, MaterialsCheckbox.IsChecked.Value);

            #else

            _userDefinedUnitTypeId = _unitsViewModel.SelectedUnit.ForgeTypeId;

            // Use our custom implementation of IExportContext as the exporter context.
            glTFExportContext ctx = new glTFExportContext(doc, filename , directoryPath, _userDefinedUnitTypeId, progressBar,  true, 
                true, FlipAxysCheckbox.IsChecked.Value, MaterialsCheckbox.IsChecked.Value);
            
            #endif

            // Create a new custom exporter with the context.
            CustomExporter exporter = new CustomExporter(doc, ctx);
                
            exporter.ShouldStopOnError = true;

            exporter.Export(view3d);

            progressBar.ViewModel.Message = "GLTF exportation completed!";
            Thread.Sleep(1000);
            progressBar.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Title_Link(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://e-verse.com/");
        }
    }
}
