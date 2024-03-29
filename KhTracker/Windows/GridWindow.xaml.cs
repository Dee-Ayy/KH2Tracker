﻿using Microsoft.Win32;
using Microsoft.VisualBasic;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Security.Cryptography;
using System.Threading;

namespace KhTracker
{
    /// <summary>
    /// Interaction logic for GridWindow.xaml
    /// </summary>
    public partial class GridWindow : Window
    {
        public bool canClose = false;
        //Dictionary<string, int> worlds = new Dictionary<string, int>();
        //Dictionary<string, int> others = new Dictionary<string, int>();
        //Dictionary<string, int> totals = new Dictionary<string, int>();
        //Dictionary<string, int> important = new Dictionary<string, int>();
        //Dictionary<string, ContentControl> Progression = new Dictionary<string, ContentControl>();
        Data data;
        public GridOptionsWindow gridOptionsWindow;
        public ColorPickerWindow colorPickerWindow;

        public int numRows;
        public int numColumns;
        public string seedName;
        public bool bingoLogic;
        public bool battleshipLogic;
        public bool fogOfWar;

        public Grid grid;
        public ToggleButton[,] buttons;
        public Color[,] originalColors;
        public bool[,] bingoStatus;
        public bool[,] annotationStatus;
        public Dictionary<string, bool> gridSettings = new Dictionary<string, bool>();
        public Dictionary<string, Color> currentColors = new Dictionary<string, Color>();
        public Dictionary<string, ContentControl> bossHintContentControls = new Dictionary<string, ContentControl>();
        public Dictionary<string, Border> bossHintBorders = new Dictionary<string, Border>();
        public List<string> assets;

        // battleship specific
        private Random random;
        public int seed;
        public int[,] placedShips;
        private List<Tuple<int, int>> possibleShipHeads;
        private List<int> shipSizes = new List<int> { 2, 3, 3, 4, 5 }; // Assuming you have this set somewhere
        private int currentShipId = 1; // Start with 1 and increment for each ship


        public GridWindow(Data dataIn)
        {
            InitializeComponent();
            InitOptions();

            gridSettings = JsonSerializer.Deserialize<Dictionary<string, bool>>(Properties.Settings.Default.GridSettings);
            currentColors = GetColorSettings();

            numRows = Properties.Settings.Default.GridWindowRows;
            numColumns = Properties.Settings.Default.GridWindowColumns;

            bingoLogic = Properties.Settings.Default.GridWindowBingoLogic;
            battleshipLogic = Properties.Settings.Default.GridWindowBattleshipLogic;
            fogOfWar = Properties.Settings.Default.FogOfWar;

            GenerateGrid(numRows, numColumns);
            //Item.UpdateTotal += new Item.TotalHandler(UpdateTotal);

            data = dataIn;
            gridOptionsWindow = new GridOptionsWindow(this, data);
            colorPickerWindow = new ColorPickerWindow(this, currentColors);

            Top = Properties.Settings.Default.GridWindowY;
            Left = Properties.Settings.Default.GridWindowX;

            Width = Properties.Settings.Default.GridWindowWidth;
            Height = Properties.Settings.Default.GridWindowHeight;
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.GridWindowY = RestoreBounds.Top;
            Properties.Settings.Default.GridWindowX = RestoreBounds.Left;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Properties.Settings.Default.GridWindowWidth = RestoreBounds.Width;
            Properties.Settings.Default.GridWindowHeight = RestoreBounds.Height;
        }

        void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            gridOptionsWindow.Hide();
            colorPickerWindow.Hide();
            if (!canClose)
            {
                e.Cancel = true;
            }
        }

        private void DownloadCardSetting(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON Files (*.json)|*.json";
            saveFileDialog.FileName = "settings.json";
            if (saveFileDialog.ShowDialog() == true)
            {
                var combinedSettings = new
                {
                    bingoLogic = bingoLogic,
                    battleshipLogic = battleshipLogic,
                    numRows = numRows,
                    numColumns = numColumns,
                    seedName = seedName,
                    gridSettings = gridSettings
                };

                var jsonString = JsonSerializer.Serialize(combinedSettings);
                System.IO.File.WriteAllText(saveFileDialog.FileName, jsonString);
            }
        }

        private void UploadCardSetting(object sender, RoutedEventArgs e)
        {

            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                DefaultExt = ".json",
                Filter = "JSON Files (*.json)|*.json",
                Title = "Select Grid Settings File",
            };


            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var jsonString = System.IO.File.ReadAllText(openFileDialog.FileName);

                    using (JsonDocument doc = JsonDocument.Parse(jsonString))
                    {

                        var root = doc.RootElement;
                        bingoLogic = root.GetProperty("bingoLogic").GetBoolean();
                        battleshipLogic = root.GetProperty("battleshipLogic").GetBoolean();
                        numRows = root.GetProperty("numRows").GetInt32();
                        numColumns = root.GetProperty("numColumns").GetInt32();
                        seedName = root.GetProperty("seedName").GetString();
                        gridSettings = JsonSerializer.Deserialize<Dictionary<string, bool>>(root.GetProperty("gridSettings").GetRawText());
                    }

                    if (SavePreviousGridSettingsOption.IsChecked) {
                        Properties.Settings.Default.GridWindowRows = numRows;
                        Properties.Settings.Default.GridWindowColumns = numColumns;
                        Properties.Settings.Default.GridSettings = JsonSerializer.Serialize<Dictionary<string, bool>>(gridSettings);
                        Properties.Settings.Default.GridWindowBingoLogic = bingoLogic;
                        Properties.Settings.Default.GridWindowBattleshipLogic = battleshipLogic;
                    }

                    // update number of reports
                    int numReports = 0;
                    for (int i = 1; i <= 13; i++)
                    {
                        if (gridSettings[$"Report{i}"])
                            numReports++;
                    }
                    Properties.Settings.Default.GridWindowNumReports = numReports;

                    // update number of unlocks
                    var unlockNames = Codes.worldUnlocks;
                    int numUnlocks = 0;
                    foreach (string unlock in unlockNames)
                    {
                        if (gridSettings[unlock])
                            numUnlocks++;
                    }
                    Properties.Settings.Default.GridWindowNumUnlocks = numUnlocks;

                    // update number of chest locks
                    var worldChestLockNames = Codes.chestLocks;
                    int numChestLocks = 0;
                    foreach (string unlock in unlockNames)
                    {
                        if (gridSettings[unlock])
                            numChestLocks++;
                    }
                    Properties.Settings.Default.GridWindowNumUnlocks = numChestLocks;
                }
                catch
                {
                    Console.WriteLine("FILE DID NOT READ CORRECTLY");
                    return;
                }
            }
            grid.Children.Clear();
            GenerateGrid(numRows, numColumns, seedName);
            // re-init the Grid OptionsWindow so that the properties of grid get re-defined
            gridOptionsWindow = new GridOptionsWindow(this, data);
            gridOptionsWindow.UpdateGridSettings(data);
            
        }

        private void SetSeedname(object sender, RoutedEventArgs e)
        {
            var inputDialog = new SeedNamer();
            if (inputDialog.ShowDialog() == true)
            {
                seedName = inputDialog.InputText;
            }
            GenerateGrid(numRows, numColumns, seedName);
        }

        private void Grid_Options(object sender, RoutedEventArgs e)
        {
       
            gridOptionsWindow.Show();

        }

        private List<string> Change_Icons(List<string> imageKeys)
        {
            if (TelevoIconsOption.IsChecked)
            {
                for (int i = 0; i < imageKeys.Count; i++)
                {
                    imageKeys[i] = imageKeys[i].Replace("Old-", "Min-");
                }
            }
            if (SonicIconsOption.IsChecked)
            {
                for (int i = 0; i < imageKeys.Count; i++)
                {
                    imageKeys[i] = imageKeys[i].Replace("Min-", "Old-");
                }
            }
            return imageKeys;
        }

        private List<string> Asset_Collection(int seed = 1)
        {

            List<ResourceDictionary> itemsDictionaries = new List<ResourceDictionary>();

            var trackableChecksDict = new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/ItemDictionary.xaml")
            };
            itemsDictionaries.Add(trackableChecksDict);

            var trackableProgressionDict = new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/ProgressionDictionary.xaml")
            };
            itemsDictionaries.Add(trackableProgressionDict);

            var trackableItemsDict = new Dictionary<object, object>();

            Random rng = new Random(seed);

            // RE-randomize which reports get included
            var numReports = Properties.Settings.Default.GridWindowNumReports;
            var randomReports = Enumerable.Range(1, 13).OrderBy(g => rng.Next()).Take(numReports).ToList();
            foreach (int reportNum in Enumerable.Range(1, 13).ToList())
                gridSettings[$"Report{reportNum}"] = randomReports.Contains(reportNum) ? true : false;

            // RE-randomize which visit unlocks get included
            var unlockNames = Codes.worldUnlocks;
            int numUnlocks = Properties.Settings.Default.GridWindowNumUnlocks;
            var randomUnlocks = Enumerable.Range(1, unlockNames.Count).OrderBy(g => rng.Next()).Take(numUnlocks).ToList();
            foreach (int i in Enumerable.Range(1, unlockNames.Count).ToList())
                gridSettings[unlockNames[i - 1]] = randomUnlocks.Contains(i) ? true : false;

            // RE-randomize which visit world chest locks get included
            var worldChestLockNames = Codes.chestLocks;
            int numChestLocks = Properties.Settings.Default.GridWindowNumChestLocks;
            var randomChestLocks = Enumerable.Range(1, worldChestLockNames.Count).OrderBy(g => rng.Next()).Take(numChestLocks).ToList();
            foreach (int i in Enumerable.Range(1, worldChestLockNames.Count).ToList())
                gridSettings[worldChestLockNames[i - 1]] = randomChestLocks.Contains(i) ? true : false;

            foreach (ResourceDictionary rd in itemsDictionaries)
            {
                foreach (DictionaryEntry entry in rd)
                {
                    if (entry.Value is GridLabelledImage img && img.GridAllowed)
                    {
                        // regardless of image style, the image ID should be consistent so we just gather all of them from Min
                        if (((string)entry.Key).Split('-')[0] == "Min")
                        {
                            // add the item to the grid settings dictionary if it doesn't exist already (IN ACCORDANCE WITH USER SETTINGS)
                            string checkName = ((string)entry.Key).Split('-')[1];
                            gridSettings[checkName] = gridSettings.ContainsKey(checkName) ? gridSettings[checkName] : img.GridAllowed;
                            if (gridSettings[checkName])
                                trackableItemsDict[checkName] = entry.Value;
                        }
                    }

                }
            }

            var randomizedItemsDict = trackableItemsDict.OrderBy(x => rng.Next()).ToDictionary(x => x.Key, x => x.Value);

            List<string> imageKeys = new List<string>();

            foreach (KeyValuePair<object, object> kvp in randomizedItemsDict)
            {
                imageKeys.Add((string)kvp.Key);
            }

            return imageKeys;
        }

        private Dictionary<string, Color> GetColorSettings()
        {

            var unmarkedColor = Properties.Settings.Default.UnmarkedColor;
            var markedColor = Properties.Settings.Default.MarkedColor;
            var annotatedColor = Properties.Settings.Default.AnnotatedColor;
            var bingoColor = Properties.Settings.Default.BingoColor;
            var hintColor = Properties.Settings.Default.HintColor;
            var battleshipMissColor = Properties.Settings.Default.BattleshipMissColor;
            var battleshipHitColor = Properties.Settings.Default.BattleshipHitColor;
            var battleshipSunkColor = Properties.Settings.Default.BattleshipSunkColor;

            return new Dictionary<string, Color>()
            {
                { "Unmarked Color", Color.FromArgb(unmarkedColor.A, unmarkedColor.R, unmarkedColor.G, unmarkedColor.B) },
                { "Marked Color", Color.FromArgb(markedColor.A, markedColor.R, markedColor.G, markedColor.B) },
                { "Annotated Color", Color.FromArgb(annotatedColor.A, annotatedColor.R, annotatedColor.G, annotatedColor.B) },
                { "Bingo Color", Color.FromArgb(bingoColor.A, bingoColor.R, bingoColor.G, bingoColor.B) },
                { "Hint Color", Color.FromArgb(hintColor.A, hintColor.R, hintColor.G, hintColor.B) },
                { "Battleship Miss Color", Color.FromArgb(battleshipMissColor.A, battleshipMissColor.R, battleshipMissColor.G, battleshipMissColor.B) },
                { "Battleship Hit Color", Color.FromArgb(battleshipHitColor.A, battleshipHitColor.R, battleshipHitColor.G, battleshipHitColor.B) },
                { "Battleship Sunk Color", Color.FromArgb(battleshipSunkColor.A, battleshipSunkColor.R, battleshipSunkColor.G, battleshipSunkColor.B) }
            };
        }


        public void Button_Click(object sender, RoutedEventArgs e, int i, int j)
        {
            var button = (ToggleButton)sender;
            if (battleshipLogic)
            {
                if (GetColorFromButton(button.Background) == currentColors["Battleship Sunk Color"])
                {
                    UnmarkSunkShips(i, j);
                }
                else if (GetColorFromButton(button.Background) == currentColors["Unmarked Color"] || GetColorFromButton(button.Background) == currentColors["Annotated Color"])
                {
                    // fix this to be conditional on the button corresponding to a ship
                    SetColorForButton(button.Background, (placedShips[i, j] != 0) ? currentColors["Battleship Hit Color"] : currentColors["Battleship Miss Color"]);
                    MarkSunkShips(i, j);
                }
                else
                {
                    SetColorForButton(button.Background, currentColors["Unmarked Color"]);
                }
            }
            else
            {
                if (GetColorFromButton(button.Background) == currentColors["Unmarked Color"] || GetColorFromButton(button.Background) == currentColors["Annotated Color"])
                {
                    SetColorForButton(button.Background, currentColors["Marked Color"]);
                }
                else
                {
                    SetColorForButton(button.Background, currentColors["Unmarked Color"]);
                }
                if (bingoLogic)
                {
                    BingoCheck(grid, i, j);
                    UpdateBingoCells();
                }
            }
            if (fogOfWar)
            {
                buttons[i, j].SetResourceReference(ContentProperty, assets[(i * numColumns) + j]);
                int westRange = 1;
                int eastRange = 1;
                int northRange = 1;
                int southRange = 1;
                int northwestRange = 3;
                int northeastRange = 3;
                int southwestRange = 3;
                int southeastRange = 3;
                // west check
                for (int west = 1; west <= westRange; west++)
                {
                    if ((j - west) >= 0)
                        buttons[i, j - west].SetResourceReference(ContentProperty, assets[(i * numColumns) + (j - west)]);
                }
                // east check
                for (int east = 1; east <= eastRange; east++)
                {
                    if ((j + east) < numColumns)
                        buttons[i, j + east].SetResourceReference(ContentProperty, assets[(i * numColumns) + (j + east)]);
                }
                // north check
                for (int north = 1; north <= northRange; north++)
                {
                    if ((i - north) >= 0)
                        buttons[i - north, j].SetResourceReference(ContentProperty, assets[((i - north) * numColumns) + j]);
                }
                for (int south = 1; south <= southRange; south++)
                {
                    // south check
                    if ((i + south) < numRows)
                        buttons[i + south, j].SetResourceReference(ContentProperty, assets[((i + south) * numColumns) + j]);
                }
                // northwest check
                for (int northwest = 1; northwest <= northwestRange; northwest++)
                {
                    if ((i - northwest) >= 0 && (j - northwest) >= 0)
                        buttons[i - northwest, j - northwest].SetResourceReference(ContentProperty, assets[((i - northwest) * numColumns) + (j - northwest)]);
                }
                // northeast check
                for (int northeast = 1; northeast <= northeastRange; northeast++)
                {
                    if ((i - northeast) >= 0 && (j + northeast) < numColumns)
                        buttons[i - northeast, j + northeast].SetResourceReference(ContentProperty, assets[((i - northeast) * numColumns) + (j + northeast)]);
                }
                // southwest check
                for (int southwest = 1; southwest <= southwestRange; southwest++)
                {
                    if ((i + southwest) < numRows && (j - southwest) >= 0)
                        buttons[i + southwest, j - southwest].SetResourceReference(ContentProperty, assets[((i + southwest) * numColumns) + (j - southwest)]);
                }
                // southeast check
                for (int southeast = 1; southeast <= southeastRange; southeast++)
                {
                    if ((i + southeast) < numRows && (j + southeast) < numColumns)
                        buttons[i + southeast, j + southeast].SetResourceReference(ContentProperty, assets[((i + southeast) * numColumns) + (j + southeast)]);
                }
            }
        }

        public void Button_RightClick(object sender, RoutedEventArgs e, int i, int j)
        {
            var button = (ToggleButton)sender;
            if (annotationStatus[i, j])
            {
                annotationStatus[i, j] = false;
                SetColorForButton(button.Background, originalColors[i, j]);
            }
            else
            {
                originalColors[i, j] = GetColorFromButton(button.Background);
                annotationStatus[i, j] = true;
                SetColorForButton(button.Background, currentColors["Annotated Color"]);
            }
        }

        public void GenerateGrid(object sender, RoutedEventArgs e)
        {
            grid.Children.Clear();
            GenerateGrid(numRows, numColumns);
        }

        public void GenerateGrid(int rows = 5, int columns = 5, string seedString = null, bool iconChange = false)
        {
            // default to 5x5 grid if negative value manages to make it in
            if (rows <= 0 || columns <= 0)
            {
                rows = 5;
                columns = 5;
            }

            grid = new Grid();
            buttons = (iconChange && buttons != null) ? buttons : new ToggleButton[rows, columns];
            originalColors = (iconChange && originalColors != null) ? originalColors : new Color[rows, columns];
            bingoStatus = (iconChange && bingoStatus != null) ? bingoStatus : new bool[rows, columns];
            annotationStatus = (iconChange && annotationStatus != null) ? annotationStatus : new bool[rows, columns];
            var randValue = new Random();
            string alphanumeric = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            seedName = seedString;

            if (seedName == null && (data?.convertedSeedHash ?? -1) > 0 && data.firstGridOnSeedLoad)
            {
                seedName = "[TIED TO SEED]";
                seed = data.convertedSeedHash;
                data.firstGridOnSeedLoad = false;
            }
            else 
            {
                if (seedName == null)
                    seedName = new string(Enumerable.Range(0, 8).Select(_ => alphanumeric[randValue.Next(alphanumeric.Length)]).ToArray());
                seed = seedName.GetHashCode();
            }
            Seedname.Header = "Seed: " + seedName;

            if (iconChange)
                // switch image style
                assets = Change_Icons(assets);
            else
            {
                // get raw check names
                assets = Asset_Collection(seed);
                // set the content resource reference with style
                string style = TelevoIconsOption.IsChecked ? "Min-" : "Old-";
                assets = assets.Select(item => style + item).ToList();
            }

            // if there aren't enough assets to fit the grid, get the grid closest to the user input that can contain all assets
            int numChecks = assets.Count;
            int originalNumRows = rows;
            int originalNumColumns = columns;
            if (rows * columns > numChecks)
            {
                while (rows * columns > numChecks)
                {
                    int currentMax = Math.Max(rows, columns);
                    if (currentMax == rows)
                        rows -= 1;
                    else
                        columns -= 1;
                }
                numRows = rows;
                numColumns = columns;
                MessageBox.Show($"NOTE: Your original request for a grid of size {originalNumRows} x {originalNumColumns} is not possible with only {numChecks} allowed checks. Grid has been reduced to size of {numRows} x {numColumns}");
            }

            // generate the grid
            for (int i = 0; i < numRows; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            }

            for (int j = 0; j < numColumns; j++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            }

            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numColumns; j++)
                {
                    ToggleButton button = new ToggleButton();
                    if (!fogOfWar)
                        button.SetResourceReference(ContentProperty, assets[(i * numColumns) + j]);
                    button.Background = new SolidColorBrush(currentColors["Unmarked Color"]);
                    button.Tag = assets[(i * numColumns) + j].ToString();
                    button.Style = (Style)FindResource("ColorToggleButton");
                    // keep i and j static for the button
                    int current_i = i;
                    int current_j = j;
                    button.Click += (sender, e) => Button_Click(sender, e, current_i, current_j);
                    button.MouseRightButtonUp += (sender, e) => Button_RightClick(sender, e, current_i, current_j);
                    Grid.SetRow(button, i);
                    Grid.SetColumn(button, j);
                    if (iconChange)
                    {
                        button.Background = buttons[i, j].Background;
                        button.IsChecked = buttons[i, j].IsChecked;
                    }
                    buttons[i, j] = button;
                    grid.Children.Add(button);
                    button.ToolTip = ((string)button.Tag).Split('-')[1];
                }
            }

            // generate battleship board
            if (battleshipLogic)
            {
                placedShips = GenerateSameBoard(numRows, numColumns);
            }

            // generate the boss hints
            bossHintContentControls = new Dictionary<string, ContentControl>();
            bossHintBorders = new Dictionary<string, Border>();
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numColumns; j++)
                {
                    // Create a new Grid as a container for the ContentControl
                    Grid hintContainer = new Grid
                    {
                        // Set the container to fill the grid cell
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch
                    };

                    // Define row definitions for the hintContainer grid
                    int coveragePercentage = 32;
                    hintContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(coveragePercentage, GridUnitType.Star) }); // coveragePercentage for the hint
                    hintContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(100 - coveragePercentage, GridUnitType.Star) }); // 100 - coveragePercentage remains empty
                    hintContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100 - coveragePercentage, GridUnitType.Star) }); // 100 - coveragePercentage remains empty
                    hintContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(coveragePercentage, GridUnitType.Star) }); // coveragePercentage for the hint

                    // Create a Border with a white background for the top right cell
                    //Border whiteBackground = new Border
                    //{
                    //    Background = new SolidColorBrush(Colors.Transparent), // Make the inside of the border transparent
                    //    BorderBrush = new SolidColorBrush(Colors.White), // Set the color of the border edges
                    //    BorderThickness = new Thickness(3), // Set the thickness of the edges
                    //                                        // The rest of the properties remain the same
                    //};
                    Border whiteBackground = new Border
                    {
                        // this will be the background when a boss hint is acquired
                        //Background = new SolidColorBrush(Colors.White),
                    };

                    string bossName = ((string)buttons[i, j].Tag).Split('-')[1];
                    bossHintBorders[bossName] = whiteBackground;

                    // Set the Border to occupy the top 35% and the right 35% of the hintContainer
                    Grid.SetRow(whiteBackground, 0);
                    Grid.SetColumn(whiteBackground, 1);
                    hintContainer.Children.Add(whiteBackground);

                    // Create the ContentControl with desired properties
                    ContentControl contentControl = new ContentControl
                    {
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                    };

                    // Naming the ContentControl using its grid position
                    bossHintContentControls[bossName] = contentControl;

                    // Add the ContentControl to the first row of the hintContainer
                    Grid.SetRow(contentControl, 0); // Place it in the top 35% row
                    Grid.SetColumn(contentControl, 1); // Place it in the right 35% column
                    hintContainer.Children.Add(contentControl);

                    // Set the hintContainer to be in the specific cell of the main grid
                    Grid.SetRow(hintContainer, i);
                    Grid.SetColumn(hintContainer, j);

                    // Add the hintContainer to the main grid, instead of directly adding the contentControl
                    grid.Children.Add(hintContainer);
                }
            }
            // Add grid to the window or other container
            DynamicGrid.Children.Add(grid);
        }

        public void SetColorForButton(Brush buttonBackground, Color newColor)
        {
            ((SolidColorBrush)buttonBackground).Color = newColor;
        }

        public Color GetColorFromButton(Brush buttonBackground)
        {
            return ((SolidColorBrush)buttonBackground).Color;
        }

        public void BingoCheck(Grid grid, int i, int j)
        {
            int rowCount = grid.RowDefinitions.Count;
            int columnCount = grid.ColumnDefinitions.Count;

            // remove any bingos if we are unclicking
            if (buttons[i, j].IsChecked == false)
            {
                // remove unclicked button's bingo status
                bingoStatus[i, j] = false;

                // check if we can have diagonal bingos
                if (rowCount == columnCount)
                {
                    // remove left diagonal
                    if (i == j)
                    {
                        for (int index = 0; index < rowCount; index++)
                        {
                            if (bingoStatus[index, index])
                            {
                                // check that the button in question is not a part of a row or column bingo before removing bingo background
                                bool partOfRowBingo = true;
                                bool partOfColumnBingo = true;
                                for (int check = 0; check < rowCount; check++)
                                {
                                    if (!bingoStatus[index, check])
                                        partOfRowBingo = false;
                                    if (!bingoStatus[check, index])
                                        partOfColumnBingo = false;
                                    if (!partOfRowBingo && !partOfColumnBingo)
                                    {
                                        if (index != i)
                                        {
                                            // check that the middle button (if it exists) is not part of the other diagonal bingo
                                            if (index * 2 == rowCount - 1)
                                            {
                                                for (int diagCheck = 0; diagCheck < rowCount; diagCheck++)
                                                {
                                                    if (!bingoStatus[diagCheck, rowCount - 1 - diagCheck])
                                                    {
                                                        bingoStatus[index, index] = false;
                                                        break;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                bingoStatus[index, index] = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // remove right diagonal
                    if (i == rowCount - 1 - j)
                    {
                        for (int index = 0; index < rowCount; index++)
                        {
                            if (bingoStatus[index, rowCount - 1 - index])
                            {
                                // check that the button in question is not a part of a row or column bingo before removing bingo background
                                bool partOfRowBingo = true;
                                bool partOfColumnBingo = true;
                                for (int check = 0; check < rowCount; check++)
                                {
                                    if (!bingoStatus[index, check])
                                        partOfRowBingo = false;
                                    if (!bingoStatus[check, rowCount - 1 - index])
                                        partOfColumnBingo = false;
                                    if (!partOfRowBingo && !partOfColumnBingo)
                                    {
                                        if (index != i)
                                        {
                                            // check that the middle button (if it exists) is not part of the other diagonal bingo
                                            if (index * 2 == rowCount - 1)
                                            {
                                                for (int diagCheck = 0; diagCheck < rowCount; diagCheck++)
                                                {
                                                    if (!bingoStatus[diagCheck, diagCheck])
                                                    {
                                                        bingoStatus[index, rowCount - 1 - index] = false;
                                                        break;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                bingoStatus[index, rowCount - 1 - index] = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                // remove vertical bingo
                for (int row = 0; row < rowCount; row++)
                {
                    bool partOfRowBingo = true;
                    bool partOfLeftDiagBingo = true;
                    bool partOfRightDiagBingo = true;

                    if (bingoStatus[row, j])
                    {
                        // check that the button in question is not a part of a row bingo before removing bingo background 
                        for (int colCheck = 0; colCheck < columnCount; colCheck++)
                        {
                            if (row != i)
                            {

                                if (!bingoStatus[row, colCheck])
                                {
                                    partOfRowBingo = false;
                                    break;
                                }
                            }
                        }
                        if (rowCount == columnCount)
                        {
                            // check that the button in question is not a part of left diagonal bingo before removing bingo background 
                            if (j == row)
                            {
                                for (int leftDiagCheck = 0; leftDiagCheck < rowCount; leftDiagCheck++)
                                {
                                    if (!bingoStatus[leftDiagCheck, leftDiagCheck])
                                    {
                                        partOfLeftDiagBingo = false;
                                        break;
                                    }
                                }
                            }
                            else
                                partOfLeftDiagBingo = false;
                            // check that the button in question is not a part of right diagonal bingo before removing bingo background
                            if (j == rowCount - 1 - row)
                            {
                                for (int rightDiagCheck = 0; rightDiagCheck < rowCount; rightDiagCheck++)
                                {
                                    if (!bingoStatus[rightDiagCheck, rowCount - 1 - rightDiagCheck])
                                    {
                                        partOfRightDiagBingo = false;
                                        break;
                                    }
                                }
                            }
                            else
                                partOfRightDiagBingo = false;
                        }
                        else
                        {
                            partOfLeftDiagBingo = false;
                            partOfRightDiagBingo = false;
                        }
                        if (!partOfRowBingo && !partOfLeftDiagBingo && !partOfRightDiagBingo)
                            bingoStatus[row, j] = false;
                    }
                }

                // remove horizontal bingo
                for (int col = 0; col < columnCount; col++)
                {
                    bool partOfColumnBingo = true;
                    bool partOfLeftDiagBingo = true;
                    bool partOfRightDiagBingo = true;

                    if (bingoStatus[i, col])
                    {
                        // check that the button in question is not a part of a column bingo before removing bingo background 
                        for (int rowCheck = 0; rowCheck < rowCount; rowCheck++)
                        {
                            if (col != j)
                            {
                                if (!bingoStatus[rowCheck, col])
                                {
                                    partOfColumnBingo = false;
                                    break;
                                }
                            }
                        }
                        if (rowCount == columnCount)
                        {
                            // check that the button in question is not a part of left diagonal bingo before removing bingo background 
                            if (i == col)
                            {
                                for (int leftDiagCheck = 0; leftDiagCheck < rowCount; leftDiagCheck++)
                                {
                                    if (!bingoStatus[leftDiagCheck, leftDiagCheck])
                                    {
                                        partOfLeftDiagBingo = false;
                                        break;
                                    }
                                }
                            }
                            else
                                partOfLeftDiagBingo = false;
                            // check that the button in question is not a part of right diagonal bingo before removing bingo background
                            if (i == rowCount - 1 - col)
                            {
                                for (int rightDiagCheck = 0; rightDiagCheck < rowCount; rightDiagCheck++)
                                {
                                    if (!bingoStatus[rightDiagCheck, rowCount - 1 - rightDiagCheck])
                                    {
                                        partOfRightDiagBingo = false;
                                        break;
                                    }
                                }
                            }
                            else
                                partOfRightDiagBingo = false;
                        }
                        else
                        {
                            partOfLeftDiagBingo = false;
                            partOfRightDiagBingo = false;
                        }
                        if (!partOfColumnBingo && !partOfLeftDiagBingo && !partOfRightDiagBingo)
                            bingoStatus[i, col] = false;
                    }
                }
            }

            // add any bingos if we are clicking
            else
            {
                // check if we can have diagonal bingos
                if (rowCount == columnCount)
                {
                    // add left diagonal
                    if (i == j)
                    {
                        for (int index = 0; index < rowCount; index++)
                        {
                            if (buttons[index, index].IsChecked == false)
                            {
                                break;
                            }
                            if (index == rowCount - 1)
                            {
                                for (int bingoIndex = 0; bingoIndex < rowCount; bingoIndex++)
                                    bingoStatus[bingoIndex, bingoIndex] = true;
                            }
                        }

                    }

                    // add right diagonal
                    if (i == rowCount - 1 - j)
                    {
                        for (int index = 0; index < rowCount; index++)
                        {
                            if (buttons[index, rowCount - 1 - index].IsChecked == false)
                            {
                                break;
                            }
                            if (index == rowCount - 1)
                            {
                                for (int bingoIndex = 0; bingoIndex < rowCount; bingoIndex++)
                                    bingoStatus[bingoIndex, rowCount - 1 - bingoIndex] = true;
                            }
                        }

                    }
                }
                // add vertical bingo
                for (int row = 0; row < rowCount; row++)
                {
                    if (buttons[row, j].IsChecked == false)
                    {
                        break;
                    }
                    if (row == rowCount - 1)
                    {
                        for (int bingoRow = 0; bingoRow < rowCount; bingoRow++)
                            bingoStatus[bingoRow, j] = true;
                    }
                }

                // add horizontal bingo
                for (int col = 0; col < columnCount; col++)
                {
                    if (buttons[i, col].IsChecked == false)
                    {
                        break;
                    }
                    if (col == columnCount - 1)
                    {
                        for (int bingoCol = 0; bingoCol < columnCount; bingoCol++)
                            bingoStatus[i, bingoCol] = true;
                    }
                }
            }
        }

        public void UpdateBingoCells()
        {
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numColumns; j++)
                {
                    if (bingoStatus[i, j])
                    {
                        SetColorForButton(buttons[i, j].Background, currentColors["Bingo Color"]);
                    }
                    else if (annotationStatus[i, j])
                    {
                        continue;
                    }
                    else if (buttons[i, j].IsChecked ?? false)
                    {
                        SetColorForButton(buttons[i, j].Background, currentColors["Marked Color"]);
                    }
                    else
                    {
                        SetColorForButton(buttons[i, j].Background, currentColors["Unmarked Color"]);
                    }
                }
            }
        }

        public int[,] GenerateSameBoard(int numRows, int numColumns)
        {
            random = new Random(data?.convertedSeedHash ?? seed);
            placedShips = new int[numRows, numColumns];

            // Initialize all possible starting points for ship heads
            possibleShipHeads = Enumerable.Range(0, numRows)
                                           .SelectMany(row => Enumerable.Range(0, numColumns), (row, col) => new Tuple<int, int>(row, col))
                                           .ToList();

            if (!TryPlaceShips(0))
            {
                throw new Exception("Failed to place all ships.");
            }

            return placedShips;
        }

        private bool TryPlaceShips(int shipIndex)
        {
            if (shipIndex >= shipSizes.Count)
            {
                return true; // All ships placed successfully
            }

            int shipSize = shipSizes[shipIndex];
            // Shuffle possible starting points to ensure random selection
            var shuffledShipHeads = possibleShipHeads.OrderBy(x => random.Next()).ToList();

            foreach (var head in shuffledShipHeads)
            {
                var directions = new List<string> { "down", "right" };
                // Shuffle directions to randomize the orientation selection
                var shuffledDirections = directions.OrderBy(x => random.Next()).ToList();

                foreach (var direction in shuffledDirections)
                {
                    if (IsDirectionValid(placedShips, head.Item1, head.Item2, shipSize, direction, numRows, numColumns))
                    {
                        PlaceShip(head, shipSize, direction);
                        if (TryPlaceShips(shipIndex + 1))
                        {
                            return true;
                        }
                        // Remove the ship if placing subsequent ships failed, effectively backtracking
                        RemoveShip(head, shipSize, direction);
                    }
                }
            }

            return false; // Unable to place this ship, prompting backtracking
        }

        private void RemoveShip(Tuple<int, int> shipHead, int shipSize, string direction)
        {
            int x = shipHead.Item1;
            int y = shipHead.Item2;

            if (direction == "down")
            {
                for (int i = 0; i < shipSize; i++)
                {
                    placedShips[x + i, y] = 0; // Mark the cell as empty
                }
            }
            else if (direction == "right")
            {
                for (int i = 0; i < shipSize; i++)
                {
                    placedShips[x, y + i] = 0; // Mark the cell as empty
                }
            }
        }

        // Your existing IsDirectionValid method goes here unchanged.

        private void PlaceShip(Tuple<int, int> shipHead, int shipSize, string direction)
        {
            int x = shipHead.Item1;
            int y = shipHead.Item2;

            if (direction == "down")
            {
                for (int i = 0; i < shipSize; i++)
                {
                    placedShips[x + i, y] = currentShipId;
                }
            }
            else if (direction == "right")
            {
                for (int i = 0; i < shipSize; i++)
                {
                    placedShips[x, y + i] = currentShipId;
                }
            }
            currentShipId++; // Move to the next ship ID for the next ship
        }


        bool IsDirectionValid(int[,] board, int x, int y, int shipSize, string direction, int numRows, int numColumns)
        {
            if (direction == "down")
            {
                if (x + shipSize > numRows) return false; // Ship goes out of bounds
                for (int i = 0; i < shipSize; i++)
                {
                    // Check if the cell is already occupied
                    if (board[x + i, y] != 0) return false;

                    // Check adjacent cells for existing ships (excluding diagonal cells)
                    if (y > 0 && board[x + i, y - 1] != 0) return false; // Left
                    if (y < numColumns - 1 && board[x + i, y + 1] != 0) return false; // Right
                    if (i == 0 && x > 0 && board[x - 1, y] != 0) return false; // Above the first cell
                    if (i == shipSize - 1 && x + shipSize < numRows && board[x + shipSize, y] != 0) return false; // Below the last cell
                }
            }
            else if (direction == "right")
            {
                if (y + shipSize > numColumns) return false; // Ship goes out of bounds
                for (int i = 0; i < shipSize; i++)
                {
                    // Check if the cell is already occupied
                    if (board[x, y + i] != 0) return false;

                    // Check adjacent cells for existing ships (excluding diagonal cells)
                    if (x > 0 && board[x - 1, y + i] != 0) return false; // Above
                    if (x < numRows - 1 && board[x + 1, y + i] != 0) return false; // Below
                    if (i == 0 && y > 0 && board[x, y - 1] != 0) return false; // Left of the first cell
                    if (i == shipSize - 1 && y + shipSize < numColumns && board[x, y + shipSize] != 0) return false; // Right of the last cell
                }
            }

            return true;
        }

        private void UnmarkSunkShips(int hitRow, int hitColumn)
        {
            int shipId = placedShips[hitRow, hitColumn];
            for (int row = 0; row < numRows; row++)
            {
                for (int column = 0; column < numColumns; column++)
                {
                    // If we find a part of the ship that has not been hit, return false
                    if (placedShips[row, column] == shipId)
                    {
                        if (row == hitRow && column == hitColumn)
                            SetColorForButton(buttons[row, column].Background, currentColors["Unmarked Color"]);
                        else
                            SetColorForButton(buttons[row, column].Background, currentColors["Battleship Hit Color"]);
                    }
                }
            }
        }

        private void MarkSunkShips(int hitRow, int hitColumn)
        {
            bool shipSunk = true;
            int shipId = placedShips[hitRow, hitColumn];

            // Iterate over the entire grid to check if any part of the ship is not hit
            for (int row = 0; row < numRows; row++)
            {
                for (int column = 0; column < numColumns; column++)
                {
                    // If we find a part of the ship that has not been hit, return false
                    if (placedShips[row, column] == shipId && !(buttons[row, column].IsChecked ?? false))
                    {
                        shipSunk = false;
                    }
                }
            }

            if (shipSunk)
            {
                for (int row = 0; row < numRows; row++)
                {
                    for (int column = 0; column < numColumns; column++)
                    {
                        // If we find a part of the ship that has not been hit, return false
                        if (placedShips[row, column] == shipId)
                        {
                            SetColorForButton(buttons[row, column].Background, currentColors["Battleship Sunk Color"]);
                        }
                    }
                }
                bool allShipsSunk = true;
                for (int row = 0; row < numRows; row++)
                {
                    for (int column = 0; column < numColumns; column++)
                    {
                        if (placedShips[row, column] != 0)
                        {
                            if (GetColorFromButton(buttons[row, column].Background) != currentColors["Battleship Sunk Color"])
                                allShipsSunk = false;
                            // stop checking for all ships sunk
                            if (!allShipsSunk)
                                break;
                        }
                    }
                    // stop checking for all ships sunk
                    if (!allShipsSunk)
                        break;
                }
                if (allShipsSunk)
                {
                    MessageBox.Show("Congrats! You sunk all ships!");
                }
            }
        }


        // updates colors upon close
        private void PickColor_Click(object sender, RoutedEventArgs e)
        {
            // prompt user for new colors
            colorPickerWindow.Show();
        }

        private void InitOptions()
        {
            // save grid settings
            SavePreviousGridSettingsOption.IsChecked = Properties.Settings.Default.SavePreviousGridSetting;
            SavePreviousGridSettingsToggle(SavePreviousGridSettingsOption.IsChecked);

            // enable televo icons
            TelevoIconsOption.IsChecked = Properties.Settings.Default.TelevoIcons;
            TelevoIconsToggle(TelevoIconsOption.IsChecked);

            // enable sonic icons
            SonicIconsOption.IsChecked = Properties.Settings.Default.SonicIcons;
            SonicIconsToggle(SonicIconsOption.IsChecked);
        }
    }
}
