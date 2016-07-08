﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AdoGemeenschap;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace AdoWPF
{
    // <summary>
    // Interaction logic for OverzichtBrouwers.xaml
    // </summary>
    public partial class OverzichtBrouwers : Window
    {
        private CollectionViewSource brouwerViewSource;
        public ObservableCollection<Brouwer> brouwersOb = new ObservableCollection<Brouwer>();
        public List<Brouwer> OudeBrouwers = new List<Brouwer>();
        public List<Brouwer> NieuweBrouwers = new List<Brouwer>();
        public List<Brouwer> GewijzigdeBrouwers = new List<Brouwer>();

        void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (Brouwer oudeBrouwer in e.OldItems)
                {
                    OudeBrouwers.Add(oudeBrouwer);
                }
            }
            if (e.NewItems != null)
            {
                foreach (Brouwer nieuweBrouwer in e.NewItems)
                {
                    NieuweBrouwers.Add(nieuweBrouwer);
                }
            }
        }
        public OverzichtBrouwers()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var manager = new BrouwerManager();
            comboBoxPostCode.Items.Add("alles");
            List<string> pc = manager.GetPostCodes();
            foreach (var p in pc)
            {
                comboBoxPostCode.Items.Add(p);
            }
            comboBoxPostCode.SelectedIndex = 0;
            VulDeGrid();
            textBoxZoeken.Focus();
        }

        private void goToFirstButton_Click(object sender, RoutedEventArgs e)
        {
            brouwerViewSource.View.MoveCurrentToFirst();
            goUpdate();
        }
        private void goToPreviousButton_Click(object sender, RoutedEventArgs e)
        {
            brouwerViewSource.View.MoveCurrentToPrevious();
            goUpdate();
        }
        private void goToNextButton_Click(object sender, RoutedEventArgs e)
        {
            brouwerViewSource.View.MoveCurrentToNext();
            goUpdate();
        }
        private void goToLastButton_Click(object sender, RoutedEventArgs e)
        {
            brouwerViewSource.View.MoveCurrentToLast();
            goUpdate();
        }

        private void goUpdate()
        {
            goToPreviousButton.IsEnabled = !(brouwerViewSource.View.CurrentPosition == 0);
            goToFirstButton.IsEnabled = !(brouwerViewSource.View.CurrentPosition == 0);
            goToNextButton.IsEnabled =
            !(brouwerViewSource.View.CurrentPosition == brouwerDataGrid.Items.Count - 2);
            goToLastButton.IsEnabled =
            !(brouwerViewSource.View.CurrentPosition == brouwerDataGrid.Items.Count - 2);

            if (brouwerDataGrid.Items.Count != 0)
            {
                if (brouwerDataGrid.SelectedItem != null)
                {
                    brouwerDataGrid.ScrollIntoView(brouwerDataGrid.SelectedItem);
                    listBoxBrouwers.ScrollIntoView(brouwerDataGrid.SelectedItem);
                }
            }
            textBoxGo.Text = (brouwerViewSource.View.CurrentPosition + 1).ToString();


        }

        private void buttonZoeken_Click(object sender, RoutedEventArgs e)
        {
            VulDeGrid();
        }

        private void VulDeGrid()
        {
            brouwerViewSource = ((CollectionViewSource)(this.FindResource("brouwerViewSource")));
            var manager = new BrouwerManager();
            brouwersOb = manager.GetBrouwersBeginNaam(textBoxZoeken.Text);
            int totalRowsCount;
            totalRowsCount = brouwersOb.Count();
            labelTotalRowCount.Content = totalRowsCount;
            brouwerViewSource.Source = brouwersOb;
            brouwersOb.CollectionChanged += this.OnCollectionChanged;
            goUpdate();
        }

        private void textBoxZoeken_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                VulDeGrid();
            }
        }

        private void goButton_Click(object sender, RoutedEventArgs e)
        {
            int position;
            int.TryParse(textBoxGo.Text, out position);
            if (position > 0 && position <= brouwerDataGrid.Items.Count)
            {
                brouwerViewSource.View.MoveCurrentToPosition(position - 1);
            }
            else
            {
                MessageBox.Show("The input index is not valid.");
            }
            goUpdate();
        }

        private void brouwerDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            goUpdate();
        }

        private void brouwerDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (CheckOpFouten()) e.Handled = true;
        }

        private void brouwerDataGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (CheckOpFouten()) e.Handled = true;
        }

        bool CheckOpFouten()
        {
            bool foutGevonden = false;
            foreach (var c in gridDetail.Children)
            {
                if (c is AdornerDecorator)
                {
                    if (Validation.GetHasError(((AdornerDecorator)c).Child))
                    {
                        foutGevonden = true;
                    }
                }
                else if (Validation.GetHasError((DependencyObject)c))
                {
                    foutGevonden = true;
                }
            }
            return foutGevonden;
        }

        private void comboBoxPostCode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxPostCode.SelectedIndex == 0)
                brouwerDataGrid.Items.Filter = null;
            else
                brouwerDataGrid.Items.Filter = new Predicate<object>(PostCodeFilter);
        }

        public bool PostCodeFilter(object br)
        {
            Brouwer b = br as Brouwer;
            bool result = (b.Postcode == Convert.ToInt16(comboBoxPostCode.SelectedValue));
            return result;
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            brouwerDataGrid.CommitEdit(DataGridEditingUnit.Row, true);
            var manager = new BrouwerManager();
            if (OudeBrouwers.Count() != 0)
            {
                manager.SchrijfVerwijderingen(OudeBrouwers);
                labelTotalRowCount.Content =
                (int)labelTotalRowCount.Content - OudeBrouwers.Count();
            }
            OudeBrouwers.Clear();
            if (NieuweBrouwers.Count() != 0)
            {
                manager.SchrijfToevoegingen(NieuweBrouwers);
                labelTotalRowCount.Content =
                (int)labelTotalRowCount.Content + NieuweBrouwers.Count();
            }
            NieuweBrouwers.Clear();

            foreach (Brouwer b in brouwersOb)
            {
                if (b.Changed == true) GewijzigdeBrouwers.Add(b);
                b.Changed = false;
            }
            if (GewijzigdeBrouwers.Count() != 0)
            {
                manager.SchrijfWijzigingen(GewijzigdeBrouwers);
            }
            GewijzigdeBrouwers.Clear();
            VulDeGrid();
            MessageBox.Show("Alles is opgeslagen in de database", "Info",
            MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
