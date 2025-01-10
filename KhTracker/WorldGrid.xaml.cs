using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace KhTracker
{
    /// <summary>
    /// Interaction logic for WorldGrid.xaml
    /// </summary>
    public partial class WorldGrid : UniformGrid
    {
        //let's simplyfy some stuff and remove a ton of redundant code
        MainWindow window = (MainWindow)App.Current.MainWindow;

        //real versions for itempool counts
        public static int Real_Fire = 0;
        public static int Real_Blizzard = 0;
        public static int Real_Thunder = 0;
        public static int Real_Cure = 0;
        public static int Real_Reflect = 0;
        public static int Real_Magnet = 0;
        public static int Real_Pages = 0;
        public static int Real_Pouches = 0;

        public static int Real_AuronWep = 0;
        public static int Real_MulanWep = 0;
        public static int Real_BeastWep = 0;
        public static int Real_JackWep = 0;
        public static int Real_SimbaWep = 0;
        public static int Real_SparrowWep = 0;
        public static int Real_AladdinWep = 0;
        public static int Real_TronWep = 0;
        public static int Real_MembershipCard = 0;
        public static int Real_IceCream = 0;
        public static int Real_RikuWep = 0;
        public static int Real_KingsLetter = 0;

        //public static int localLevelCount = 0;
        public static int Ghost_Fire = 0;
        public static int Ghost_Blizzard = 0;
        public static int Ghost_Thunder = 0;
        public static int Ghost_Cure = 0;
        public static int Ghost_Reflect = 0;
        public static int Ghost_Magnet = 0;
        public static int Ghost_Pages = 0;
        public static int Ghost_Pouches = 0;

        public static int Ghost_AuronWep = 0;
        public static int Ghost_MulanWep = 0;
        public static int Ghost_BeastWep = 0;
        public static int Ghost_JackWep = 0;
        public static int Ghost_SimbaWep = 0;
        public static int Ghost_SparrowWep = 0;
        public static int Ghost_AladdinWep = 0;
        public static int Ghost_TronWep = 0;
        public static int Ghost_MembershipCard = 0;
        public static int Ghost_IceCream = 0;
        public static int Ghost_RikuWep = 0;
        public static int Ghost_KingsLetter = 0;

        //amount of obtained ghost magic/pages
        public static int Ghost_Fire_obtained = 0;
        public static int Ghost_Blizzard_obtained = 0;
        public static int Ghost_Thunder_obtained = 0;
        public static int Ghost_Cure_obtained = 0;
        public static int Ghost_Reflect_obtained = 0;
        public static int Ghost_Magnet_obtained = 0;
        public static int Ghost_Pages_obtained = 0;
        public static int Ghost_Pouches_obtained = 0;

        public static int Ghost_AuronWep_obtained = 0;
        public static int Ghost_MulanWep_obtained = 0;
        public static int Ghost_BeastWep_obtained = 0;
        public static int Ghost_JackWep_obtained = 0;
        public static int Ghost_SimbaWep_obtained = 0;
        public static int Ghost_SparrowWep_obtained = 0;
        public static int Ghost_AladdinWep_obtained = 0;
        public static int Ghost_TronWep_obtained = 0;
        public static int Ghost_MembershipCard_obtained = 0;
        public static int Ghost_IceCream_obtained = 0;
        public static int Ghost_RikuWep_obtained = 0;
        public static int Ghost_KingsLetter_obtained = 0;

        //track other types of collections
        public static int Proof_Count = 0;
        public static int Form_Count = 0;
        public static int Summon_Count = 0;
        public static int Ability_Count = 0;
        public static int Report_Count = 0;
        public static int Visit_Count = 0;

        //A single spot to have referenced for the opacity of the ghost checks idk where to put this
        public static double universalOpacity = 0.5;

        private int worldRowSize = 7;

        public WorldGrid()
        {
            InitializeComponent();

            if (Properties.Settings.Default.ClassicRowSize)
            {
                this.Columns = 5;
                worldRowSize = 5;
            }
        }

        public void Handle_WorldGrid(Item button, bool add)
        {
            Data data = MainWindow.data;
            int addRemove = 1;

            if (add)
            {
                //Default should be children count so that items are
                //always added to the end if no ghosts are found
                int firstGhost = Children.Count;

                //search for ghost items
                foreach (Item child in Children)
                {
                    if (child.Name.StartsWith("Ghost_"))
                    {
                        //when one is found get the index of it
                        firstGhost = Children.IndexOf(child);
                        break;
                    }
                }

                //add the item
                try
                {
                    Children.Insert(firstGhost, button);
                }
                catch (Exception)
                {
                    return;
                }

            }
            else
            {
                Children.Remove(button);
                addRemove = -1;
            }

            UpdateGhostObtained(button, addRemove);
            UpdateMulti(button, add);

            int gridremainder = 0;
            if (Children.Count % worldRowSize != 0)
                gridremainder = 1;

            int gridnum = Math.Max((Children.Count / worldRowSize) + gridremainder, 1);
            Rows = gridnum;

            // default 1, add .5 for every row
            double length = 1 + ((Children.Count - 1) / worldRowSize) / 2.0;
            Grid outerGrid = (Parent as Grid).Parent as Grid;
            int row = (int)Parent.GetValue(Grid.RowProperty);
            outerGrid.RowDefinitions[row].Height = new GridLength(length, GridUnitType.Star);
            string worldName = Name.Substring(0, Name.Length - 4);

            //visit lock check first
            if (window.VisitLockOption.IsChecked)
            {
                SetVisitLock(button.Name, add);
            }

            if (data.mode == Mode.ShanHints || data.mode == Mode.OpenKHShanHints || data.mode == Mode.PathHints)
            {
                WorldComplete();

                if (data.WorldsData[worldName].value != null)
                {
                    window.SetWorldValue(data.WorldsData[worldName].value, Children.Count);
                }
            }

            if (data.mode == Mode.PointsHints)
            {
                if (button.Name.StartsWith("Ghost_"))
                    SetWorldGhost(worldName);
                else
                    WorldComplete();

                //Console.WriteLine(button.Name + ": " + worldName + " added/removed " + (TableReturn(button.Name) * addRemove));

                window.SetPoints(worldName, window.GetPoints(worldName) - (TableReturn(button.Name) * addRemove));
                window.SetWorldValue(MainWindow.data.WorldsData[worldName].value, window.GetPoints(worldName));

                //remove ghost items as needed then update points score
                if (worldName != "GoA" && !button.Name.StartsWith("Ghost_"))
                {
                    if (add)
                    {
                        Remove_Ghost(worldName, button);
                    }

                    window.UpdatePointScore(TableReturn(button.Name) * addRemove);
                }
            }

            if (data.mode == Mode.SpoilerHints)
            {
                if (data.SpoilerWorldCompletion && !button.Name.StartsWith("Ghost_"))
                    WorldComplete();

                //remove ghost items as needed
                if (worldName != "GoA" && !button.Name.StartsWith("Ghost_") && add)
                {
                    Remove_Ghost(worldName, button);
                }

                if (data.WorldsData[worldName].value != null)
                {
                    //Get count - ghosts
                    int realcount = 0;

                    foreach (Item item in Children)
                    {
                        if (item.Name.StartsWith("Ghost"))
                            continue;
                        else
                            realcount += 1;
                    }

                    //Set world value
                    window.SetWorldValue(data.WorldsData[worldName].value, realcount);
                }
            }

            if (data.ScoreMode)
            {
                if (worldName != "GoA" && !button.Name.StartsWith("Ghost_"))
                {
                    window.UpdatePointScore(TableReturn(button.Name) * addRemove);
                }
            }
        }

        private void Item_Drop(Object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(Item)))
            {
                Item item = e.Data.GetData(typeof(Item)) as Item;

                if (ReportHandler(item))
                    Add_Item(item);
            }
        }

        public void Add_Item(Item item)
        {
            //remove item from itempool
            Grid ItemRow = null;
            try
            {
                ItemRow = VisualTreeHelper.GetParent(item) as Grid;
            }
            catch
            {
                return;
            }

            if (ItemRow == null || ItemRow.Parent != window.ItemPool)
                return;

            ItemRow.Children.Remove(item);

            //add it to the world grid
            Handle_WorldGrid(item, true);

            //fix shadow opacity if needed
            Handle_Shadows(item, true);

            //Reset any obtained item to be normal transparency
            item.Opacity = 1.0;

            // update collection count
            window.SetCollected(true);

            // update mouse actions
            if (MainWindow.data.dragDrop)
            {
                item.MouseDoubleClick -= item.Item_Click;
                item.MouseMove -= item.Item_MouseMove;
            }
            else
            {
                item.MouseDown -= item.Item_MouseDown;
                item.MouseUp -= item.Item_MouseUp;
            }
            item.MouseDown -= item.Item_Return;
            item.MouseDown += item.Item_Return;

            Grid_Add_Item(item, false);
        }

        //Marks sure the grid tracker always tracks items sets in order
        //(fire as fire1, fira as fire2, ect.)
        //this was done because it was relying soely on the item name itself which had the possibility of
        //being tracked in any order, especially with manual tracking
        public void Grid_Add_Item(Item item, bool gridOnly)
        {
            //do nothing for ghost items
            if (item.Name.StartsWith("Ghost_"))
                return;

            //need to update real multi counts if item is not enabled in the main window before grid tracking starts
            if (gridOnly)
                UpdateMulti(item, true);

            string itemType = Codes.FindItemType(item.Name);
            string NewName = item.Name;
            string modifier = "";

            //check if item is a multi type and get the correct number modifier for it
            if (item.Name.Contains("Munny") || itemType == "magic" || itemType == "page" || itemType == "visit")
            {
                char[] numbers = { '1', '2', '3', '4', '5' };
                NewName = item.Name.TrimEnd(numbers);

                switch (NewName)
                {
                    case "Fire":
                        modifier = Real_Fire.ToString();
                        break;
                    case "Blizzard":
                        modifier = Real_Blizzard.ToString();
                        break;
                    case "Thunder":
                        modifier = Real_Thunder.ToString();
                        break;
                    case "Cure":
                        modifier = Real_Cure.ToString();
                        break;
                    case "Reflect":
                        modifier = Real_Reflect.ToString();
                        break;
                    case "Magnet":
                        modifier = Real_Magnet.ToString();
                        break;
                    case "TornPage":
                        modifier = Real_Pages.ToString();
                        break;
                    case "MunnyPouch":
                        modifier = Real_Pouches.ToString();
                        break;
                    case "RikuWep":
                        modifier = Real_RikuWep.ToString();
                        break;
                    case "MembershipCard":
                        modifier = Real_MembershipCard.ToString();
                        break;
                    case "KingsLetter":
                        modifier = Real_KingsLetter.ToString();
                        break;
                    case "IceCream":
                        modifier = Real_IceCream.ToString();
                        break;
                    case "BeastWep":
                        modifier = Real_BeastWep.ToString();
                        break;
                    case "JackWep":
                        modifier = Real_JackWep.ToString();
                        break;
                    case "SimbaWep":
                        modifier = Real_SimbaWep.ToString();
                        break;
                    case "AuronWep":
                        modifier = Real_AuronWep.ToString();
                        break;
                    case "MulanWep":
                        modifier = Real_MulanWep.ToString();
                        break;
                    case "SparrowWep":
                        modifier = Real_SparrowWep.ToString();
                        break;
                    case "AladdinWep":
                        modifier = Real_AladdinWep.ToString();
                        break;
                    case "TronWep":
                        modifier = Real_TronWep.ToString();
                        break;
                    default:
                        break;
                }
            }

            window.UpdateSupportingTrackers(NewName+modifier);
        }

        public void UpdateMulti(Item item, bool add)
        {
            //do nothing for ghost items
            if (item.Name.StartsWith("Ghost_"))
                return;

            int addRemove = 1;
            if (!add)
                addRemove = -1;

            if (Codes.FindItemType(item.Name) != "magic" && Codes.FindItemType(item.Name) != "page" && !item.Name.Contains("Munny") && Codes.FindItemType(item.Name) != "visit")    //Codes.FindItemType(item.Name) != "other")
            {
                //yeah just gonna do things here..
                //track collection for things that aren't multi's
                switch (Codes.FindItemType(item.Name))
                {
                    case "proof":
                        Proof_Count += addRemove;
                        return;
                    case "form":
                        Form_Count += addRemove;
                        return;
                    case "ability":
                        Ability_Count += addRemove;
                        return;
                    case "summon":
                        Summon_Count += addRemove;
                        return;
                    case "visit":
                        Visit_Count += addRemove;
                        return;
                    case "report":
                        Report_Count += addRemove;
                        return;
                    //collecting the other aux checks does nothing for now. maybe someday
                    //case "other":
                    //    Aux_Count += addRemove;
                    //    return;
                    default:
                        return;
                }
            }

            char[] numbers = { '1', '2', '3', '4', '5' };
            string itemname = item.Name.TrimEnd(numbers);

            switch (itemname)
            {
                case "Fire":
                    Real_Fire += addRemove;
                    window.FireCount.Text = (3 - Real_Fire).ToString();
                    if (Real_Fire == 3)
                    {
                        window.FireCount.Fill = (SolidColorBrush)FindResource("Color_Black");
                        window.FireCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
                    }
                    else
                    {
                        window.FireCount.Fill = (LinearGradientBrush)FindResource("Color_Fire");
                        window.FireCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
                    }
                    return;
                case "Blizzard":
                    Real_Blizzard += addRemove;
                    window.BlizzardCount.Text = (3 - Real_Blizzard).ToString();
                    if (Real_Blizzard == 3)
                    {
                        window.BlizzardCount.Fill = (SolidColorBrush)FindResource("Color_Black");
                        window.BlizzardCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
                    }
                    else
                    {
                        window.BlizzardCount.Fill = (LinearGradientBrush)FindResource("Color_Blizzard");
                        window.BlizzardCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
                    }
                    return;
                case "Thunder":
                    Real_Thunder += addRemove;
                    window.ThunderCount.Text = (3 - Real_Thunder).ToString();
                    if (Real_Thunder == 3)
                    {
                        window.ThunderCount.Fill = (SolidColorBrush)FindResource("Color_Black");
                        window.ThunderCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
                    }
                    else
                    {
                        window.ThunderCount.Fill = (LinearGradientBrush)FindResource("Color_Thunder");
                        window.ThunderCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
                    }
                    return;
                case "Cure":
                    Real_Cure += addRemove;
                    window.CureCount.Text = (3 - Real_Cure).ToString();
                    if (Real_Cure == 3)
                    {
                        window.CureCount.Fill = (SolidColorBrush)FindResource("Color_Black");
                        window.CureCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
                    }
                    else
                    {
                        window.CureCount.Fill = (LinearGradientBrush)FindResource("Color_Cure");
                        window.CureCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
                    }
                    return;
                case "Magnet":
                    Real_Magnet += addRemove;
                    window.MagnetCount.Text = (3 - Real_Magnet).ToString();
                    if (Real_Magnet == 3)
                    {
                        window.MagnetCount.Fill = (SolidColorBrush)FindResource("Color_Black");
                        window.MagnetCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
                    }
                    else
                    {
                        window.MagnetCount.Fill = (LinearGradientBrush)FindResource("Color_Magnet");
                        window.MagnetCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
                    }
                    return;
                case "Reflect":
                    Real_Reflect += addRemove;
                    window.ReflectCount.Text = (3 - Real_Reflect).ToString();
                    if (Real_Reflect == 3)
                    {
                        window.ReflectCount.Fill = (SolidColorBrush)FindResource("Color_Black");
                        window.ReflectCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
                    }
                    else
                    {
                        window.ReflectCount.Fill = (LinearGradientBrush)FindResource("Color_Reflect");
                        window.ReflectCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
                    }
                    return;
                case "TornPage":
                    Real_Pages += addRemove;
                    window.PageCount.Text = (5 - Real_Pages).ToString();
                    if (Real_Pages == 5)
                    {
                        window.PageCount.Fill = (SolidColorBrush)FindResource("Color_Black");
                        window.PageCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
                    }
                    else
                    {
                        window.PageCount.Fill = (LinearGradientBrush)FindResource("Color_Page");
                        window.PageCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
                    }
                    return;
                case "MunnyPouch":
                    Real_Pouches += addRemove;
                    window.MunnyCount.Text = (2 - Real_Pouches).ToString();
                    if (Real_Pouches == 2)
                    {
                        window.MunnyCount.Fill = (SolidColorBrush)FindResource("Color_Black");
                        window.MunnyCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
                    }
                    else
                    {
                        window.MunnyCount.Fill = (LinearGradientBrush)FindResource("Color_Pouch");
                        window.MunnyCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
                    }
                    return;
                case "BeastWep":
                    Real_BeastWep += addRemove;
                    window.BCCount.Text = (2 - Real_BeastWep).ToString();
                    if (Real_BeastWep == 2)
                    {
                        window.BCCount.Fill = (SolidColorBrush)FindResource("Color_Black");
                        window.BCCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
                    }
                    else
                    {
                        window.BCCount.Fill = (LinearGradientBrush)FindResource("Color_BC");
                        window.BCCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
                    }
                    return;
                case "JackWep":
                    Real_JackWep += addRemove;
                    window.HTCount.Text = (2 - Real_JackWep).ToString();
                    if (Real_JackWep == 2)
                    {
                        window.HTCount.Fill = (SolidColorBrush)FindResource("Color_Black");
                        window.HTCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
                    }
                    else
                    {
                        window.HTCount.Fill = (LinearGradientBrush)FindResource("Color_HT");
                        window.HTCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
                    }
                    return;
                case "SimbaWep":
                    Real_SimbaWep += addRemove;
                    window.PLCount.Text = (2 - Real_SimbaWep).ToString();
                    if (Real_SimbaWep == 2)
                    {
                        window.PLCount.Fill = (SolidColorBrush)FindResource("Color_Black");
                        window.PLCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
                    }
                    else
                    {
                        window.PLCount.Fill = (LinearGradientBrush)FindResource("Color_PL");
                        window.PLCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
                    }
                    return;
                case "AuronWep":
                    Real_AuronWep += addRemove;
                    window.OCCount.Text = (2 - Real_AuronWep).ToString();
                    if (Real_AuronWep == 2)
                    {
                        window.OCCount.Fill = (SolidColorBrush)FindResource("Color_Black");
                        window.OCCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
                    }
                    else
                    {
                        window.OCCount.Fill = (LinearGradientBrush)FindResource("Color_OC");
                        window.OCCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
                    }
                    return;
                case "MulanWep":
                    Real_MulanWep += addRemove;
                    window.LoDCount.Text = (2 - Real_MulanWep).ToString();
                    if (Real_MulanWep == 2)
                    {
                        window.LoDCount.Fill = (SolidColorBrush)FindResource("Color_Black");
                        window.LoDCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
                    }
                    else
                    {
                        window.LoDCount.Fill = (LinearGradientBrush)FindResource("Color_LoD");
                        window.LoDCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
                    }
                    return;
                case "SparrowWep":
                    Real_SparrowWep += addRemove;
                    window.PRCount.Text = (2 - Real_SparrowWep).ToString();
                    if (Real_SparrowWep == 2)
                    {
                        window.PRCount.Fill = (SolidColorBrush)FindResource("Color_Black");
                        window.PRCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
                    }
                    else
                    {
                        window.PRCount.Fill = (LinearGradientBrush)FindResource("Color_PR");
                        window.PRCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
                    }
                    return;
                case "AladdinWep":
                    Real_AladdinWep += addRemove;
                    window.AGCount.Text = (2 - Real_AladdinWep).ToString();
                    if (Real_AladdinWep == 2)
                    {
                        window.AGCount.Fill = (SolidColorBrush)FindResource("Color_Black");
                        window.AGCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
                    }
                    else
                    {
                        window.AGCount.Fill = (LinearGradientBrush)FindResource("Color_AG");
                        window.AGCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
                    }
                    return;
                case "TronWep":
                    Real_TronWep += addRemove;
                    window.SPCount.Text = (2 - Real_TronWep).ToString();
                    if (Real_TronWep == 2)
                    {
                        window.SPCount.Fill = (SolidColorBrush)FindResource("Color_Black");
                        window.SPCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
                    }
                    else
                    {
                        window.SPCount.Fill = (LinearGradientBrush)FindResource("Color_SP");
                        window.SPCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
                    }
                    return;
                case "RikuWep":
                    Real_RikuWep += addRemove;
                    window.TWTNWCount.Text = (2 - Real_RikuWep).ToString();
                    if (Real_RikuWep == 2)
                    {
                        window.TWTNWCount.Fill = (SolidColorBrush)FindResource("Color_Black");
                        window.TWTNWCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
                    }
                    else
                    {
                        window.TWTNWCount.Fill = (LinearGradientBrush)FindResource("Color_TWTNW");
                        window.TWTNWCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
                    }
                    return;
                case "MembershipCard":
                    Real_MembershipCard += addRemove;
                    window.HBCount.Text = (2 - Real_MembershipCard).ToString();
                    if (Real_MembershipCard == 2)
                    {
                        window.HBCount.Fill = (SolidColorBrush)FindResource("Color_Black");
                        window.HBCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
                    }
                    else
                    {
                        window.HBCount.Fill = (LinearGradientBrush)FindResource("Color_HB");
                        window.HBCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
                    }
                    return;
                case "KingsLetter":
                    Real_KingsLetter += addRemove;
                    window.DCCount.Text = (2 - Real_KingsLetter).ToString();
                    if (Real_KingsLetter == 2)
                    {
                        window.DCCount.Fill = (SolidColorBrush)FindResource("Color_Black");
                        window.DCCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
                    }
                    else
                    {
                        window.DCCount.Fill = (LinearGradientBrush)FindResource("Color_DC");
                        window.DCCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
                    }
                    return;
                case "IceCream":
                    Real_IceCream += addRemove;
                    window.TTCount.Text = (3 - Real_IceCream).ToString();
                    if (Real_IceCream == 3)
                    {
                        window.TTCount.Fill = (SolidColorBrush)FindResource("Color_Black");
                        window.TTCount.Stroke = (SolidColorBrush)FindResource("Color_Trans");
                    }
                    else
                    {
                        window.TTCount.Fill = (LinearGradientBrush)FindResource("Color_TT");
                        window.TTCount.Stroke = (SolidColorBrush)FindResource("Color_Black");
                    }
                    return;
                default:
                    return;
            }
        }

        ///
        /// Report handling 
        ///

        public bool ReportHandler(Item item)
        {
            Data data = MainWindow.data;
            //we use this to check if a report is valid to be tracked before placing it in the world grid.
            //an incorrect report will update its fail status and return false
            //if any other item then alwys return true and skip any report related code.
            //(maybe this can be simplified?

            // item is a report
            if (data.hintsLoaded && item.Name.StartsWith("Report"))
            {
                //index used to get correct report info.
                //we just remove "Report" from the item name and parse the left over number as int -1 since lists are 0 based.
                int index = int.Parse(item.Name.Remove(0, 6)) - 1;

                // out of report attempts
                if (data.reportAttempts[index] == 0)
                    return false;

                //check to see if report is in the itempool.
                //if it's not then assume it's already tracked and do nothing else
                Grid ItemRow = VisualTreeHelper.GetParent(item) as Grid;
                if (ItemRow == null || ItemRow.Parent != window.ItemPool)
                    return true;

                // check for correct report location then run report hint logic based on current hint mode
                if (data.reportLocations[index] == Name.Substring(0, Name.Length - 4))
                {
                    //for progression hints
                    if (data.UsingProgressionHints)
                    {
                        //return without doing anything else
                        if (data.reportLocationsUsed[index])
                            return true;

                        //give points
                        window.AddProgressionPoints(data.ReportBonus);
                    }

                    switch (data.mode)
                    {
                        case Mode.JsmarteeHints:
                        case Mode.OpenKHJsmarteeHints:
                            Report_Jsmartee(index);
                            break;
                        case Mode.ShanHints:
                        case Mode.OpenKHShanHints:
                            Report_Shan(index, item);
                            break;
                        case Mode.PointsHints:
                            Report_Points(index);
                            break;
                        case Mode.PathHints:
                            Report_Path(index);
                            break;
                        case Mode.SpoilerHints:
                            Report_Spoiler(index);
                            break;
                        default:
                            window.SetHintText("Impossible Report Error! How are you seeing this?");
                            return false;
                    }

                    //report hover logic (with progression boss hitns reports should work as normal)
                    if (data.mode != Mode.ShanHints || data.progressionType == "Bosses")
                    {
                        item.MouseEnter -= item.Report_Hover;
                        item.MouseEnter += item.Report_Hover;
                    }

                    data.reportLocationsUsed[index] = true;
                }
                else
                {
                    // update fail icons when location is report location is wrong
                    data.reportAttempts[index]--;
                    switch (data.reportAttempts[index])
                    {
                        case 2:
                            data.ReportAttemptVisual[index].SetResourceReference(ContentControl.ContentProperty, "Fail1");
                            break;
                        case 1:
                            data.ReportAttemptVisual[index].SetResourceReference(ContentControl.ContentProperty, "Fail2");
                            break;
                        case 0:
                        default:
                            data.ReportAttemptVisual[index].SetResourceReference(ContentControl.ContentProperty, "Fail3");
                            break;
                    }
                    return false;
                }
            }

            //prog shan specific
           //if (item.Name.StartsWith("Report") && data.mode == Mode.OpenKHShanHints && data.UsingProgressionHints)
           //{
           //    int index = int.Parse(item.Name.Remove(0, 6)) - 1;
           //
           //    if (!data.reportLocationsUsed[index])
           //    {
           //        window.AddProgressionPoints(data.ReportBonus);
           //        data.reportLocationsUsed[index] = true;
           //    }
           //    else
           //    {
           //        //check if the report was already obtained before giving points
           //        if (!data.reportLocationsUsed[index])
           //        {
           //            window.AddProgressionPoints(data.ReportBonus);
           //            data.reportLocationsUsed[index] = true;
           //        }
           //        // show hint text on report hover
           //        item.MouseEnter -= item.Report_Hover;
           //        item.MouseEnter += item.Report_Hover;
           //    }
           //}

            return true;
        }

        private void Report_Shan(int index, Item item)
        {
            Data data = MainWindow.data;

            if (data.UsingProgressionHints)
            {
                if (!data.reportLocationsUsed[index])
                {
                    window.AddProgressionPoints(data.ReportBonus);
                    data.reportLocationsUsed[index] = true;
                }
                else
                {
                    //check if the report was already obtained before giving points
                    if (!data.reportLocationsUsed[index])
                    {
                        window.AddProgressionPoints(data.ReportBonus);
                        data.reportLocationsUsed[index] = true;
                    }
                    // show hint text on report hover
                    item.MouseEnter -= item.Report_Hover;
                    item.MouseEnter += item.Report_Hover;
                }
            }

            // resetting fail icons
            data.ReportAttemptVisual[index].SetResourceReference(ContentControl.ContentProperty, "Fail0");
            data.reportAttempts[index] = 3;
        }

        public void Report_Jsmartee(int index, bool fromProgression = false)
        {
            Data data = MainWindow.data;

            if(fromProgression)
            {
                // hint text
                data.HintRevealsStored.Add(new Tuple<string, string, string, bool, bool, bool>(Codes.GetHintTextName(data.reportInformation[index].Item2), "has", data.reportInformation[index].Item3 + " important checks", true, false, true));
                window.SetHintText(Codes.GetHintTextName(data.reportInformation[index].Item2), "has", data.reportInformation[index].Item3 + " important checks", true, false, true);

                // auto update world important check number
                window.SetWorldValue(data.WorldsData[data.reportInformation[index].Item2].value, data.reportInformation[index].Item3);

                return;
            }

            // resetting fail icons
            data.ReportAttemptVisual[index].SetResourceReference(ContentControl.ContentProperty, "Fail0");
            data.reportAttempts[index] = 3;

            //return because reports shouldn't give hints themselves with proggression hints
            if (data.UsingProgressionHints && data.progressionType == "Reports")
                return;
                
            // hint text
            window.SetHintText(Codes.GetHintTextName(data.reportInformation[index].Item2), "has", data.reportInformation[index].Item3 + " important checks", true, false, true);

            // set world report hints to as hinted then checks if the report location was hinted to set if its a hinted hint
            data.WorldsData[data.reportInformation[index].Item2].hinted = true;

            if (data.WorldsData[data.reportLocations[index]].hinted == true)
            {
                data.WorldsData[data.reportInformation[index].Item2].hintedHint = true;
            }

            // loop through hinted world for reports to set their info as hinted hints
            for (int i = 0; i < data.WorldsData[data.reportInformation[index].Item2].worldGrid.Children.Count; ++i)
            {
                Item gridItem = data.WorldsData[data.reportInformation[index].Item2].worldGrid.Children[i] as Item;
                if (gridItem.Name.Contains("Report"))
                {
                    int reportIndex = int.Parse(gridItem.Name.Substring(6)) - 1;
                    data.WorldsData[data.reportInformation[reportIndex].Item2].hintedHint = true;
                    window.SetWorldValue(data.WorldsData[data.reportInformation[reportIndex].Item2].value, data.reportInformation[reportIndex].Item3);
                }
            }

            // auto update world important check number
            window.SetWorldValue(data.WorldsData[data.reportInformation[index].Item2].value, data.reportInformation[index].Item3);

            //put here to remind myself later
            //else if (data.reportLocations[index] == "Joke")
            //{
            //    // hint text
            //    //window.SetJokeText(data.reportInformation[index].Item2);
            //    isreport = true;
            //}
        }

        private void Report_Points(int index)
        {
            Data data = MainWindow.data;

            // hint text
            window.SetHintText(Codes.GetHintTextName(data.reportInformation[index].Item1), "has", Codes.FindShortName(data.reportInformation[index].Item2), true, false, true);
            CheckGhost(data.reportInformation[index].Item1, data.reportInformation[index].Item2, "Report" + index);

            //resetting fail icons
            data.ReportAttemptVisual[index].SetResourceReference(ContentControl.ContentProperty, "Fail0");
            data.reportAttempts[index] = 3;
        }

        public void Report_Path(int index, bool fromProgression = false)
        {
            Data data = MainWindow.data;

            if(!fromProgression)
            {
                // resetting fail icons
                data.ReportAttemptVisual[index].SetResourceReference(ContentControl.ContentProperty, "Fail0");
                data.reportAttempts[index] = 3;

                //return because reports shouldn't give hints themselves with proggression hints
                if (data.UsingProgressionHints && data.progressionType == "Reports")
                    return;
            }
            else 
            {
                data.HintRevealsStored.Add(new Tuple<string, string, string, bool, bool, bool>(Codes.GetHintTextName(data.reportInformation[index].Item1), "", "", false, false, false));
            }

            // hint text and proof icon display
            window.SetHintText(Codes.GetHintTextName(data.reportInformation[index].Item1));
            PathProofToggle(data.reportInformation[index].Item2, data.reportInformation[index].Item3);
        }

        public void Report_Spoiler(int index, bool fromProgression = false)
        {
            Data data = MainWindow.data;

            // resetting fail icons
            if (!fromProgression)
            {
                data.ReportAttemptVisual[index].SetResourceReference(ContentControl.ContentProperty, "Fail0");
                data.reportAttempts[index] = 3;

                //return because reports shouldn't give hints themselves with proggression hints
                if (data.UsingProgressionHints && data.progressionType == "Reports")
                    return;
            }

            // hint text
            if (data.reportInformation[index].Item1 == "Empty") //normal reports
            {
                window.SetHintText("This report reveals nothing...");
            }
            else
            {
                //set alt text for a hinted world that has 0 checks
                //(for when a world is toggled on, but happens to contain nothing)
                if (data.reportInformation[index].Item3 == -1)
                {
                    window.SetHintText(Codes.GetHintTextName(data.reportInformation[index].Item1), "has no Important Checks", "", true, false, false);
                }
                else if (data.reportInformation[index].Item3 == -12345)
                {
                    if (data.UsingProgressionHints)
                        window.SetHintTextRow2(data.reportInformation[index].Item1, "became", data.reportInformation[index].Item2);
                    else
                        window.SetHintText(data.reportInformation[index].Item1, "became", data.reportInformation[index].Item2, false, false, false, true);
                }
                else if (data.reportInformation[index].Item3 == -12346)
                {
                    if (data.UsingProgressionHints)
                    {
                        window.SetHintTextRow2(data.reportInformation[index].Item1, data.reportInformation[index].Item2, "");
                    }
                    else
                        window.SetHintText(data.reportInformation[index].Item1, data.reportInformation[index].Item2, "", false, false, false, true);
                }
                else if (data.reportInformation[index].Item3 == -999)
                {
                    ///nothing...
                }
                else
                {
                    window.SetHintText(Codes.GetHintTextName(data.reportInformation[index].Item1), "has been revealed!", "", true, false, false);
                    SpoilerWorldReveal(data.reportInformation[index].Item1, "Report" + index);
                }
            }

            //change hinted world to use green numbers
            //(we do this here instead of using SetWorldGhost cause we want world numbers to stay green until they are actually complete)
            if (data.SpoilerReportMode && data.reportInformation[index].Item1 != "Empty")
            {
                data.WorldsData[data.reportInformation[index].Item1].containsGhost = true;

                if (data.WorldsData[data.reportInformation[index].Item1].containsGhost)
                    window.SetWorldValue(data.WorldsData[data.reportInformation[index].Item1].value, int.Parse(data.WorldsData[data.reportInformation[index].Item1].value.Text));
            }
        }

        public void Handle_GridTrackerHints_BE(string gridOriginalBoss, string gridNewBoss, string iconStyle = "Min")
        {

            Data data = MainWindow.data;

            // handle 1 hour hinting: use the original boss image for arena hints
            if (data.oneHourMode && data.codes.oneHourReplacements.ContainsValue(gridOriginalBoss))
            {
                var origBossKey = data.codes.oneHourReplacements
                                    .FirstOrDefault(pair => pair.Value.Equals(gridOriginalBoss))
                                    .Key;
                gridOriginalBoss = data.codes.bossNameConversion[origBossKey];
            }

            // get the hint color
            Color hintColor = window.gridWindow.currentColors["Hint Color"];

            // hint visual on grid tracker
            if (window.gridWindow.bossHintContentControls.Keys.Contains(gridNewBoss))
            {
                window.gridWindow.bossHintBorders[gridNewBoss].Background = new SolidColorBrush(hintColor);
                if (window.TryFindResource($"Grid_{iconStyle}-Grid{gridOriginalBoss}") != null)
                {
                    // Try to set the resource reference with the "Grid" prefix
                    window.gridWindow.bossHintContentControls[gridNewBoss].SetResourceReference(ContentControl.ContentProperty, $"Grid_{iconStyle}-Grid{gridOriginalBoss}");
                }
                else if (window.TryFindResource($"Grid_{iconStyle}-{gridOriginalBoss}") != null)
                {
                    // If the "Grid" key doesn't exist, try with the base key
                    window.gridWindow.bossHintContentControls[gridNewBoss].SetResourceReference(ContentControl.ContentProperty, $"Grid_{iconStyle}-{gridOriginalBoss}");

                }
            }

            else if (window.gridWindow.bossHintContentControls.Keys.Contains($"Grid{gridNewBoss}"))
            {
                window.gridWindow.bossHintBorders[$"Grid{gridNewBoss}"].Background = new SolidColorBrush(hintColor);
                if (window.TryFindResource($"Grid_{iconStyle}-Grid{gridOriginalBoss}") != null)
                {
                    // Try to set the resource reference with the "Grid" prefix
                    window.gridWindow.bossHintContentControls[$"Grid{gridNewBoss}"].SetResourceReference(ContentControl.ContentProperty, $"Grid_{iconStyle}-Grid{gridOriginalBoss}");
                }
                else if (window.TryFindResource($"Grid_{iconStyle}-{gridOriginalBoss}") != null)
                {
                    // If the "Grid" key doesn't exist, try with the base key
                    window.gridWindow.bossHintContentControls[$"Grid{gridNewBoss}"].SetResourceReference(ContentControl.ContentProperty, $"Grid_{iconStyle}-{gridOriginalBoss}");
                }
            }

            for (int row = 0; row < window.gridWindow.numRows; row++)
            {
                for (int col = 0; col < window.gridWindow.numColumns; col++)
                {
                    foreach (var checkName in new[] { gridNewBoss, $"Grid{gridNewBoss}" })
                    {
                        if ((string)window.gridWindow.buttons[row, col].Tag == checkName)
                        {
                            if (window.TryFindResource($"Grid_{iconStyle}-Grid{gridNewBoss}") != null)
                            {
                                // Try to set the resource reference with the "Grid" prefix
                                window.gridWindow.buttons[row, col].SetResourceReference(ContentControl.ContentProperty, $"Grid_{iconStyle}-Grid{gridNewBoss}");
                            }
                            else if (window.TryFindResource($"Grid_{iconStyle}-{gridNewBoss}") != null)
                            {
                                // If the "Grid" key doesn't exist, try with the base key
                                window.gridWindow.buttons[row, col].SetResourceReference(ContentControl.ContentProperty, $"Grid_{iconStyle}-{gridNewBoss}");
                            }
                        }
                    }
                }
            }
        }

        public void ProgBossHint(int index)
        {
            Data data = MainWindow.data;

            string originalBoss;
            string middle;
            string newBoss;

            if (data.BossHomeHinting)
            {
                originalBoss = data.HintRevealsStored[index].Item1;
                middle = data.HintRevealsStored[index].Item2;
                newBoss = data.HintRevealsStored[index].Item3;

                //change names for these bosses only for 1hr mode
                if (data.oneHourMode)
                {
                    if (originalBoss.Contains("Cloud"))
                    {
                        originalBoss = "Jafar (Cloud)";
                        //if (text2 == "is unchanged")
                        //{
                        //    text2 = "became";
                        //    text3 = "Cloud";
                        //}
                    }
                    if (originalBoss.Contains("Tifa"))
                    {
                        originalBoss = "Shadow Stalker (Tifa)";
                        //if (text2 == "is unchanged")
                        //{
                        //    text2 = "became";
                        //    text3 = "Tifa";
                        //}
                    }
                    if (originalBoss.Contains("Hercules"))
                    {
                        originalBoss = "Hydra (Hercules)";
                        //if (text2 == "is unchanged")
                        //{
                        //    text2 = "became";
                        //    text3 = "Hercules";
                        //}
                    }
                    if (originalBoss.Contains("Leon"))
                    {
                        originalBoss = "Twilight Thorn (Leon)";
                        //if (text2 == "is unchanged")
                        //{
                        //    text2 = "became";
                        //    text3 = "Leon";
                        //}
                    }
                    if (originalBoss.Contains("Yuffie"))
                    {
                        originalBoss = "Storm Rider (Yuffie)";
                        //if (text2 == "is unchanged")
                        //{
                        //    text2 = "became";
                        //    text3 = "Yuffie";
                        //}
                    }
                }
                window.SetHintTextRow2(originalBoss, middle, newBoss);
            }
            
            else
            {
                originalBoss = data.progBossInformation[index].Item1;
                middle = data.progBossInformation[index].Item2;
                newBoss = data.progBossInformation[index].Item3;

                // hint text
                window.SetHintTextRow2(originalBoss, middle, newBoss);
                data.HintRevealsStored.Add(new Tuple<string, string, string, bool, bool, bool>(originalBoss, middle, newBoss, false, false, false));
            }


            if (middle == "is unchanged")
            {
                newBoss = originalBoss;
            }

            //visualize hint in gridtracker
            if (data.codes.bossNameConversion.Keys.Contains(newBoss) && data.codes.bossNameConversion.Keys.Contains(originalBoss))
            {

                string gridNewBoss = data.codes.bossNameConversion[newBoss];

                // handle Pete since he has 2 versions
                if (newBoss == "Pete")
                {
                    gridNewBoss = data.BossList.ContainsValue("Pete OC II") ? "OCPete" : "DCPete";
                }

                string gridOriginalBoss = data.codes.bossNameConversion[originalBoss];

                // handle boss hint on grid tracker
                Handle_GridTrackerHints_BE(gridOriginalBoss, gridNewBoss, window.gridWindow.TelevoIconsOption.IsChecked ? "Min" : "Old");

                // handle boss hint home for data org members
                if (data.BossRandoFound && window.gridWindow.bunterLogic)
                {
                    gridOriginalBoss = gridOriginalBoss.Replace("Data", "").Replace("Axel", "Axel II");

                    if (data.BossList.ContainsKey(gridOriginalBoss)
                        && data.codes.bossNameConversion.ContainsKey(data.BossList[gridOriginalBoss]))
                    {
                        gridNewBoss = data.codes.bossNameConversion[data.BossList[gridOriginalBoss]];
                        Console.WriteLine($"{gridOriginalBoss}, {gridNewBoss}");

                        Handle_GridTrackerHints_BE(
                            gridOriginalBoss,
                            gridNewBoss,
                            window.gridWindow.TelevoIconsOption?.IsChecked == true ? "Min" : "Old"
                        );
                    }
                }
            }

        }

        ///
        /// world value handling
        ///

        private int TableReturn(string nameButton)
        {
            Data data = MainWindow.data;
            string type = Codes.FindItemType(nameButton);
            if (type != "Unknown")
            {
                return nameButton.StartsWith("Ghost_") && !window.GhostMathOption.IsChecked ? 0 : data.PointsDatanew[type];
            }
            return 0;
        }

        ///
        /// ghost item handling
        ///

        public void Add_Ghost(Item item)
        {
            Data data = MainWindow.data;
            //check if we even want to track a ghost item.
            if (window.GhostItemOption.IsChecked || data.mode == Mode.SpoilerHints)
            {
                //check item parent and track only if the parent is the itempool grid
                if (VisualTreeHelper.GetParent(item) is Grid ItemRow && ItemRow.Parent == window.ItemPool)
                {
                    ItemRow.Children.Remove(item);
                    Handle_WorldGrid(item, true);
                }
            }
        }

        private void Remove_Ghost(string world, Item item)
        {
            Data data = MainWindow.data;
            //check to see if world currently contains a ghost

            //if Points JsmarteeHints and world doesn't contain a ghost yet, do nothing and return
            if (!data.WorldsData[world].containsGhost && data.mode == Mode.PointsHints)
                return;

            //If spoiler hints, check if ANY currently tracked item in this world is a ghost
            //and return and do nothing if there are none.
            bool hasGhost = false;
            if (data.mode == Mode.SpoilerHints)
            {
                foreach (Item child in data.WorldsData[world].worldGrid.Children)
                {
                    if (child.Name.StartsWith("Ghost_"))
                    {
                        hasGhost = true;
                        break;
                    }
                }

                if (!hasGhost)
                    return;
            }

            //get correct item name
            char[] numbers = { '1', '2', '3', '4', '5' };
            string itemname = item.Name;
            if (Codes.FindItemType(item.Name) == "magic" || Codes.FindItemType(item.Name) == "page" || item.Name.StartsWith("Munny") || Codes.FindItemType(item.Name) == "visit")
            {
                itemname = itemname.TrimEnd(numbers);
            }

            foreach (var child in Children)
            {
                Item ghostItem = child as Item;
                string itemnameGhost;
                if (ghostItem != null && ghostItem.Name.StartsWith("Ghost_"))
                {
                    itemnameGhost = ghostItem.Name;
                    bool isMulti = false;

                    //trim numbers
                    if (Codes.FindItemType(ghostItem.Name) == "magic" || Codes.FindItemType(ghostItem.Name) == "page" || item.Name.StartsWith("Munny") || Codes.FindItemType(item.Name) == "visit")
                    {
                        itemnameGhost = itemnameGhost.TrimEnd(numbers);
                        isMulti = true;
                    }

                    if (!isMulti)
                    {
                        Handle_Shadows(item, false);
                    }

                    //compare and remove if same
                    if (itemname == itemnameGhost.Remove(0, 6))
                    {
                        ghostItem.HandleItemReturn();
                        return;
                    }
                }
            }
        }

        private void CheckGhost(string world, string item, string report)
        {
            Data data = MainWindow.data;
            //don't bother checking if ghost tracking is off
            if (window.GhostItemOption.IsChecked == false)
                return;

            //check if report was tracked before in this session to avoid tracking multiple ghosts for removing and placing the same report back
            if (data.TrackedReports.Contains(report))
                return;
            else
                data.TrackedReports.Add(report);

            //parse item name
            string itemname = Codes.ConvertSeedGenName(item);

            //this shouldn't ever happen, but return without doing anything else if the ghost values for magic/pages are higher than expected
            switch (itemname)
            {
                case "Fire":
                    if (Ghost_Fire >= 3)
                    {
                        return;
                    }
                    break;
                case "Blizzard":
                    if (Ghost_Blizzard >= 3)
                    {
                        return;
                    }
                    break;
                case "Thunder":
                    if (Ghost_Thunder >= 3)
                    {
                        return;
                    }
                    break;
                case "Cure":
                    if (Ghost_Cure >= 3)
                    {
                        return;
                    }
                    break;
                case "Magnet":
                    if (Ghost_Magnet >= 3)
                    {
                        return;
                    }
                    break;
                case "Reflect":
                    if (Ghost_Reflect >= 3)
                    {
                        return;
                    }
                    break;
                case "TornPage":
                    if (Ghost_Pages >= 5)
                    {
                        return;
                    }
                    break;
                case "MunnyPouch":
                    if (Ghost_Pouches >= 2)
                    {
                        return;
                    }
                    break;
                default:
                    break;
            }

            //cycle through hinted world's checks for items
            char[] numbers = { '1', '2', '3', '4', '5' };
            List<string> CurrentGhosts = new List<string>();
            List<string> CurrentItems = new List<string>();
            foreach (var child in data.WorldsData[world].worldGrid.Children)
            {
                Item ItemCheck = child as Item;
                string itemName = ItemCheck.Name;

                //trim numbers if needed
                if (Codes.FindItemType(ItemCheck.Name) == "magic" || Codes.FindItemType(ItemCheck.Name) == "page" || Codes.FindItemType(ItemCheck.Name) == "other" || Codes.FindItemType(ItemCheck.Name) == "visit")
                {
                    itemName = itemName.TrimEnd(numbers);
                }
                //update lists
                if (ItemCheck.Name.StartsWith("Ghost_"))
                {
                    //avoid adding a ghost entirely if a ghost of the same type is found (for magic and pages)
                    if (CurrentGhosts.Contains(itemName))
                        continue;
                    else
                        CurrentGhosts.Add(itemName);
                }
                else
                {
                    //avoid adding an item entirely if one of the same type is found (for magic and pages)
                    if (CurrentItems.Contains(itemName))
                        continue;
                    else
                        CurrentItems.Add(itemName);
                }
            }

            //compare hinted item to current list of ghosts
            if (CurrentGhosts.Contains("Ghost_" + itemname) || CurrentItems.Contains(itemname))
            {
                return;
            }

            //look for avaiable ghost item in item pool to track
            Grid ItemRow = VisualTreeHelper.GetChild(window.ItemPool, window.ItemPool.Children.Count - 1) as Grid;
            foreach (Item Ghost in ItemRow.Children)
            {
                if (Ghost != null && Ghost.Name.Contains("Ghost_" + itemname))
                {
                    //found ghost item, let's track it and break
                    data.WorldsData[world].worldGrid.Add_Ghost(Ghost);
                    break;
                }
            }
        }

        private void Handle_Shadows(Item item, bool add)
        {
            //don't hide shadows for the multi items
            if (Codes.FindItemType(item.Name) == "magic" || Codes.FindItemType(item.Name) == "page" || item.Name.StartsWith("Munny") || item.Name.StartsWith("Ghost_") || Codes.FindItemType(item.Name) == "visit")
            {
                return;
            }

            string shadowName = "S_" + item.Name;
            //ContentControl shadow = window.ItemPool.FindName(shadowName) as ContentControl;
            ContentControl shadow = null;

            foreach (Grid itemrow in window.ItemPool.Children)
            {
                shadow = itemrow.FindName(shadowName) as ContentControl;

                if (shadow != null)
                    break;
            }

            if (shadow == null)
                return;

            if (add)
            {
                shadow.Opacity = 1.0;
            }
            else
            {
                shadow.Opacity = 0.0;
            }
        }

        private void SpoilerWorldReveal(string worldname, string report)
        {
            Data data = MainWindow.data;
            //check if report was tracked before in this session to avoid tracking
            //multiple ghosts for removing and placing the same report back
            if (data.TrackedReports.Contains(report))
                return;
            else
                data.TrackedReports.Add(report);

            //create temp list of what a world should have
            List<string> tempWorldItems = new List<string>();
            tempWorldItems.AddRange(data.WorldsData[worldname].hintedItemList);
            char[] numbers = { '1', '2', '3', '4', '5' };

            //Get list of items we should track. we don't want to place more ghosts than is needed
            //(ex. a world has 2 blizzards and we already have 1 tracked there)
            WorldGrid worldGrid = data.WorldsData[worldname].worldGrid;
            foreach (Item item in worldGrid.Children)
            {
                //just skip if item is a ghost. hintedItemList should never contain ghosts anyway
                if (item.Name.StartsWith("Ghost_"))
                    continue;

                //do not trim numbers if report
                if (item.Name.Contains("Report") && tempWorldItems.Contains(item.Name))
                    tempWorldItems.Remove(item.Name);
                else if (tempWorldItems.Contains(item.Name.TrimEnd(numbers)))
                {
                    tempWorldItems.Remove(item.Name.TrimEnd(numbers));
                }
            }

            //start tracking what's left in the temp list
            foreach (string itemname in tempWorldItems)
            {
                //don't track item types not set in reveal list
                if (!data.SpoilerRevealTypes.Contains(Codes.FindItemType(itemname)))
                {
                    continue;
                }

                //this shouldn't ever happen, but return without doing anything else if the ghost values for magic/pages are higher than expected
                switch (itemname)
                {
                    case "Fire":
                        if (Ghost_Fire >= 3)
                        {
                            return;
                        }
                        break;
                    case "Blizzard":
                        if (Ghost_Blizzard >= 3)
                        {
                            return;
                        }
                        break;
                    case "Thunder":
                        if (Ghost_Thunder >= 3)
                        {
                            return;
                        }
                        break;
                    case "Cure":
                        if (Ghost_Cure >= 3)
                        {
                            return;
                        }
                        break;
                    case "Magnet":
                        if (Ghost_Magnet >= 3)
                        {
                            return;
                        }
                        break;
                    case "Reflect":
                        if (Ghost_Reflect >= 3)
                        {
                            return;
                        }
                        break;
                    case "TornPage":
                        if (Ghost_Pages >= 5)
                        {
                            return;
                        }
                        break;
                    case "MunnyPouch":
                        if (Ghost_Pouches >= 2)
                        {
                            return;
                        }
                        break;
                    default:
                        break;
                }

                //look for avaiable ghost item in item pool to track
                //Note: for now the ghost items are always the 4th itemgrid.
                Grid ItemRow = VisualTreeHelper.GetChild(window.ItemPool, window.ItemPool.Children.Count - 1) as Grid;
                foreach (Item Ghost in ItemRow.Children)
                {
                    //Console.WriteLine(Ghost.Name);
                    if (Ghost != null && Ghost.Name.Contains("Ghost_" + itemname))
                    {
                        //found ghost item
                        data.WorldsData[worldname].worldGrid.Add_Ghost(Ghost);
                        break;
                    }
                }
            }
        }

        private void SetWorldGhost(string worldName)
        {
            Data data = MainWindow.data;
            foreach (Item child in Children)
            {
                if (data.GhostItems.Values.Contains(child))
                {
                    data.WorldsData[worldName].containsGhost = true;
                    return;
                }
                else
                {
                    data.WorldsData[worldName].containsGhost = false;
                }
            }
        }

        private void UpdateGhostObtained(Item item, int addremove)
        {
            Data data = MainWindow.data;
            //return if mod isn't either of these
            if (data.mode != Mode.PointsHints && data.mode != Mode.SpoilerHints)
            {
                return;
            }

            char[] numbers = { '1', '2', '3', '4', '5' };
            string itemntype = Codes.FindItemType(item.Name);
            string itemname;

            if (item.Name.Contains("Report"))
                itemname = item.Name;
            else
                itemname = item.Name.TrimEnd(numbers);

            //update normal items obtained
            if (!itemname.StartsWith("Ghost_"))
            {
                switch (itemname)
                {
                    case "Fire":
                        Ghost_Fire_obtained += addremove;
                        break;
                    case "Blizzard":
                        Ghost_Blizzard_obtained += addremove;
                        break;
                    case "Thunder":
                        Ghost_Thunder_obtained += addremove;
                        break;
                    case "Cure":
                        Ghost_Cure_obtained += addremove;
                        break;
                    case "Reflect":
                        Ghost_Reflect_obtained += addremove;
                        break;
                    case "Magnet":
                        Ghost_Magnet_obtained += addremove;
                        break;
                    case "TornPage":
                        Ghost_Pages_obtained += addremove;
                        break;
                    case "MunnyPouch":
                        Ghost_Pouches_obtained += addremove;
                        break;
                    case "BeastWep":
                        Ghost_BeastWep_obtained += addremove;
                        break;
                    case "AuronWep":
                        Ghost_AuronWep_obtained += addremove;
                        break;
                    case "MulanWep":
                        Ghost_MulanWep_obtained += addremove;
                        break;
                    case "JackWep":
                        Ghost_JackWep_obtained += addremove;
                        break;
                    case "SimbaWep":
                        Ghost_SimbaWep_obtained += addremove;
                        break;
                    case "SparrowWep":
                        Ghost_SparrowWep_obtained += addremove;
                        break;
                    case "AladdinWep":
                        Ghost_AladdinWep_obtained += addremove;
                        break;
                    case "TronWep":
                        Ghost_TronWep_obtained += addremove;
                        break;
                    case "MembershipCard":
                        Ghost_MembershipCard_obtained += addremove;
                        break;
                    case "IceCream":
                        Ghost_IceCream_obtained += addremove;
                        break;
                    case "RikuWep":
                        Ghost_RikuWep_obtained += addremove;
                        break;
                    case "KingsLetter":
                        Ghost_KingsLetter_obtained += addremove;
                        break;
                    default:
                        break;
                }
            }

            //update ghost items hinted
            if (itemname.StartsWith("Ghost_"))
            {
                switch (itemname)
                {
                    case "Ghost_Fire":
                        Ghost_Fire += addremove;
                        break;
                    case "Ghost_Blizzard":
                        Ghost_Blizzard += addremove;
                        break;
                    case "Ghost_Thunder":
                        Ghost_Thunder += addremove;
                        break;
                    case "Ghost_Cure":
                        Ghost_Cure += addremove;
                        break;
                    case "Ghost_Reflect":
                        Ghost_Reflect += addremove;
                        break;
                    case "Ghost_Magnet":
                        Ghost_Magnet += addremove;
                        break;
                    case "Ghost_TornPage":
                        Ghost_Pages += addremove;
                        break;
                    case "Ghost_MunnyPouch":
                        Ghost_Pouches += addremove;
                        break;
                    case "Ghost_BeastWep":
                        Ghost_BeastWep += addremove;
                        break;
                    case "Ghost_AuronWep":
                        Ghost_AuronWep += addremove;
                        break;
                    case "Ghost_MulanWep":
                        Ghost_MulanWep += addremove;
                        break;
                    case "Ghost_JackWep":
                        Ghost_JackWep += addremove;
                        break;
                    case "Ghost_SimbaWep":
                        Ghost_SimbaWep += addremove;
                        break;
                    case "Ghost_SparrowWep":
                        Ghost_SparrowWep += addremove;
                        break;
                    case "Ghost_AladdinWep":
                        Ghost_AladdinWep += addremove;
                        break;
                    case "Ghost_TronWep":
                        Ghost_TronWep += addremove;
                        break;
                    case "Ghost_MembershipCard":
                        Ghost_MembershipCard += addremove;
                        break;
                    case "Ghost_IceCream":
                        Ghost_IceCream += addremove;
                        break;
                    case "Ghost_RikuWep":
                        Ghost_RikuWep += addremove;
                        break;
                    case "Ghost_KingsLetter":
                        Ghost_KingsLetter += addremove;
                        break;
                    default:
                        break;
                }
            }

            SetItemPoolGhosts(itemname, itemntype);
        }

        private void SetItemPoolGhosts(string item, string type)
        {
            if (!item.StartsWith("Ghost_"))
                return;

            int GhostIC = 0;
            int ObtainedIC = 0;
            OutlinedTextBlock magicValue = null;
            //Grid ItemPool = window.ItemPool;
            Data data = MainWindow.data;

            //don't change report opacity
            if (type == "report")
            {
                return;
            }

            //simplier icon opacity change for non pages/magic
            if (type != "magic" && type != "page" && !item.Contains("Munny"))
            {
                if(type == "visit" && !item.Contains("Sketches"))
                {
                    //do nothing
                }
                else
                {
                    //check if a ghost item was tracked
                    if (item.StartsWith("Ghost_"))
                    {
                        //remove ghost prefix
                        item = item.Remove(0, 6);

                        //get item and world grid it's supposed to be in
                        Grid tempRow = data.Items[item].Item2;
                        Item Check = tempRow.FindName(item) as Item;

                        //check to see if item is *in* the ItemPool
                        if (Check != null && Check.Parent == tempRow)
                        {
                            Check.Opacity = universalOpacity; //change opacity

                            Handle_Shadows(Check, false);
                        }
                        return;
                    }
                    return;
                }
            }

            ///
            ///I shouldn't be messing with magic/page opacity right now
            ///

            //figure out what kinda item we are working with
            switch (item)
            {
                case "Ghost_Fire":
                case "Fire":
                    GhostIC = Ghost_Fire;
                    ObtainedIC = Ghost_Fire_obtained;
                    magicValue = window.Ghost_FireCount;
                    break;
                case "Ghost_Blizzard":
                case "Blizzard":
                    GhostIC = Ghost_Blizzard;
                    ObtainedIC = Ghost_Blizzard_obtained;
                    magicValue = window.Ghost_BlizzardCount;
                    break;
                case "Ghost_Thunder":
                case "Thunder":
                    GhostIC = Ghost_Thunder;
                    ObtainedIC = Ghost_Thunder_obtained;
                    magicValue = window.Ghost_ThunderCount;
                    break;
                case "Ghost_Cure":
                case "Cure":
                    GhostIC = Ghost_Cure;
                    ObtainedIC = Ghost_Cure_obtained;
                    magicValue = window.Ghost_CureCount;
                    break;
                case "Ghost_Reflect":
                case "Reflect":
                    GhostIC = Ghost_Reflect;
                    ObtainedIC = Ghost_Reflect_obtained;
                    magicValue = window.Ghost_ReflectCount;
                    break;
                case "Ghost_Magnet":
                case "Magnet":
                    GhostIC = Ghost_Magnet;
                    ObtainedIC = Ghost_Magnet_obtained;
                    magicValue = window.Ghost_MagnetCount;
                    break;
                case "Ghost_TornPage":
                case "TornPage":
                    GhostIC = Ghost_Pages;
                    ObtainedIC = Ghost_Pages_obtained;
                    magicValue = window.Ghost_PageCount;
                    break;
                case "Ghost_MunnyPouch":
                case "MunnyPouch":
                    GhostIC = Ghost_Pouches;
                    ObtainedIC = Ghost_Pouches_obtained;
                    magicValue = window.Ghost_MunnyCount;
                    break;
                case "Ghost_BeastWep":
                case "BeastWep":
                    GhostIC = Ghost_BeastWep;
                    ObtainedIC = Ghost_BeastWep_obtained;
                    magicValue = window.Ghost_BCCount;
                    break;
                case "Ghost_JackWep":
                case "JackWep":
                    GhostIC = Ghost_JackWep;
                    ObtainedIC = Ghost_JackWep_obtained;
                    magicValue = window.Ghost_HTCount;
                    break;
                case "Ghost_SimbaWep":
                case "SimbaWep":
                    GhostIC = Ghost_SimbaWep;
                    ObtainedIC = Ghost_SimbaWep_obtained;
                    magicValue = window.Ghost_PLCount;
                    break;
                case "Ghost_AuronWep":
                case "AuronWep":
                    GhostIC = Ghost_AuronWep;
                    ObtainedIC = Ghost_AuronWep_obtained;
                    magicValue = window.Ghost_OCCount;
                    break;
                case "Ghost_MulanWep":
                case "MulanWep":
                    GhostIC = Ghost_MulanWep;
                    ObtainedIC = Ghost_MulanWep_obtained;
                    magicValue = window.Ghost_LoDCount;
                    break;
                case "Ghost_SparrowWep":
                case "SparrowWep":
                    GhostIC = Ghost_SparrowWep;
                    ObtainedIC = Ghost_SparrowWep_obtained;
                    magicValue = window.Ghost_PRCount;
                    break;
                case "Ghost_AladdinWep":
                case "AladdinWep":
                    GhostIC = Ghost_AladdinWep;
                    ObtainedIC = Ghost_AladdinWep_obtained;
                    magicValue = window.Ghost_AGCount;
                    break;
                case "Ghost_TronWep":
                case "TronWep":
                    GhostIC = Ghost_TronWep;
                    ObtainedIC = Ghost_TronWep_obtained;
                    magicValue = window.Ghost_SPCount;
                    break;
                case "Ghost_RikuWep":
                case "RikuWep":
                    GhostIC = Ghost_RikuWep;
                    ObtainedIC = Ghost_RikuWep_obtained;
                    magicValue = window.Ghost_TWTNWCount;
                    break;
                case "Ghost_MembershipCard":
                case "MembershipCard":
                    GhostIC = Ghost_MembershipCard;
                    ObtainedIC = Ghost_MembershipCard_obtained;
                    magicValue = window.Ghost_HBCount;
                    break;
                case "Ghost_KingsLetter":
                case "KingsLetter":
                    GhostIC = Ghost_KingsLetter;
                    ObtainedIC = Ghost_KingsLetter_obtained;
                    magicValue = window.Ghost_DCCount;
                    break;
                case "Ghost_IceCream":
                case "IceCream":
                    GhostIC = Ghost_IceCream;
                    ObtainedIC = Ghost_IceCream_obtained;
                    magicValue = window.Ghost_TTCount;
                    break;
                default:
                    Console.WriteLine("Something went wrong? item wasn't expected. Item: " + item);
                    return;
            }

            if (magicValue != null)
            {
                if (GhostIC == 0)
                    magicValue.Visibility = Visibility.Hidden;
                else
                    magicValue.Visibility = Visibility.Visible;

                magicValue.Text = GhostIC.ToString();
            }

            //return and do nothing if the actual obtained number of items is maxed
            //if ((type == "page" && ObtainedIC == 5) || (type == "magic" && ObtainedIC == 3))
            //{
            //    GhostIC = 0;
            //    return;
            //}

            //if (item.StartsWith("Ghost_"))
            //    item = item.Remove(0, 6);

            //int Count = 3;
            //if (type == "page")
            //    Count = 5;

            //reset opacity and add items to a temp list
            //List<string> foundChecks = new List<string>();
            //for (int i = 1; i <= Count; i++)
            //{
            //    string checkName = item + i.ToString();
            //    Item check = data.Items[checkName].Item1;
            //    Grid grid = data.Items[checkName].Item2;
            //
            //    if (check != null && check.Parent == grid)
            //    {
            //        check.Opacity = 1.0;
            //        foundChecks.Add(check.Name);
            //    }
            //}

            //if (GhostIC > foundChecks.Count)
            //{
            //    Console.WriteLine("Ghost Count is greater than number of items left in itempool! How did this happen?");
            //    return;
            //}

            //calculate opacity again (for dynamic change on adding removing checks
            //for (int i = 1; i <= GhostIC; i++)
            //{
            //    Grid ItemRow = VisualTreeHelper.GetChild(ItemPool, GetItemPool[foundChecks[i - 1]]) as Grid;
            //    Item Check = ItemRow.FindName(foundChecks[i-1]) as Item;
            //    if (Check != null)
            //    {
            //        Check.Opacity = universalOpacity;
            //    }
            //}
        }

        ///
        /// world/grid visual updating
        ///

        public void WorldComplete()
        {          
            Data data = MainWindow.data;
              
            //run a check for current world to check if all checks have been found

            //get worldname by rmoving "Grid" from the end of the current worldgrid name
            string worldName = Name.Substring(0, Name.Length - 4);

            //if GoA or if the complete flag has been set. if so just return
            if (worldName == "GoA" || data.WorldsData[worldName].complete)
                return;

            //create a temp list for what checks a world should have
            List<string> tempItems = new List<string>();
            tempItems.AddRange(data.WorldsData[worldName].hintedItemList);

            //for each item currently tracked to worldgrid we remove it from the temp list
            char[] numbers = { '1', '2', '3', '4', '5' };
            foreach (var child in Children)
            {
                Item item = child as Item;

                //just skip if item is a ghost. hintedItemList should never contain ghosts anyway
                if (item.Name.StartsWith("Ghost_"))
                    continue;

                //do not trim numbers if report
                if (item.Name.Contains("Report") && tempItems.Contains(item.Name))
                    tempItems.Remove(item.Name);
                else if (tempItems.Contains(item.Name.TrimEnd(numbers)))
                {
                    tempItems.Remove(item.Name.TrimEnd(numbers));
                }
            }

            //if Templist is empty then worl can be marked as complete
            if (tempItems.Count == 0)
            {
                //Console.WriteLine("~~~~~ MARKING WORLD AS COMPLETE");
                data.WorldsData[worldName].complete = true;
                //when a world is found as complete, give the stored bonuses
                if (data.UsingProgressionHints && data.hintsLoaded)
                {
                    if (data.calulating)
                        data.ProgressionPoints += data.StoredWorldCompleteBonus[worldName];
                    else
                        window.AddProgressionPoints(data.StoredWorldCompleteBonus[worldName]);
                }
            }
        }

        public void WorldCompleteProgressionBonus()
        {
            Data data = MainWindow.data;
            //run a check for current world to check if all checks have been found

            //get worldname by rmoving "Grid" from the end of the current worldgrid name
            string worldName = Name.Substring(0, Name.Length - 4);

            //if GoA or if the complete flag has been set. if so just return
            if (worldName == "GoA")
                return;

            //if world was marked at some point as complete
            if (data.WorldsData[worldName].complete)
            {
                data.StoredWorldCompleteBonus[worldName] += data.WorldCompleteBonus;
            }
        }

        private void SetVisitLock(string itemName, bool add)
        {
            Data data = MainWindow.data;
            char[] numbers = { '1', '2', '3', '4', '5' };

            itemName = itemName.TrimEnd(numbers);

            switch (itemName)
            {
                case "BeastWep":
                    SetLockValue("BeastsCastle", add);
                    break;
                case "JackWep":
                    SetLockValue("HalloweenTown", add);
                    break;
                case "SimbaWep":
                    SetLockValue("PrideLands", add);
                    break;
                case "AuronWep":
                    SetLockValue("OlympusColiseum", add);
                    break;
                case "MulanWep":
                    SetLockValue("LandofDragons", add);
                    break;
                case "SparrowWep":
                    SetLockValue("PortRoyal", add);
                    break;
                case "AladdinWep":
                    SetLockValue("Agrabah", add);
                    break;
                case "TronWep":
                    SetLockValue("SpaceParanoids", add);
                    break;
                case "RikuWep":
                    SetLockValue("TWTNW", add);
                    break;
                case "MembershipCard":
                    SetLockValue("HollowBastion", add);
                    break;
                case "KingsLetter":
                    SetLockValue("DisneyCastle", add);
                    break;
                case "IceCream":
                    SetLockValue("TwilightTown", add);
                    break;
                case "Sketches":
                    SetLockValue("SimulatedTwilightTown", add);
                    break;
                default:
                    return;
            }

            window.VisitLockCheck();
        }

        private void SetLockValue(string worldName, bool add)
        {
            //reminder 100 = 3rd visit locked | 10 = 2nd visit locked | 1 = 1st visit locked
            //relleay should redo how this works as i don't need to wory about tt having two items that unlock different visits
            int currentValue = MainWindow.data.WorldsData[worldName].visitLocks;
            
            if (add)
            {
                if (currentValue == 0)
                    MainWindow.data.WorldsData[worldName].visitLocks += 1;
                if (currentValue == 1)
                    MainWindow.data.WorldsData[worldName].visitLocks += 10;
                if (currentValue == 11)
                    MainWindow.data.WorldsData[worldName].visitLocks += 100;
            }
            else
            {
                if (currentValue == 111)
                    MainWindow.data.WorldsData[worldName].visitLocks -= 100;
                if (currentValue == 11)
                    MainWindow.data.WorldsData[worldName].visitLocks -= 10;
                if (currentValue == 1)
                    MainWindow.data.WorldsData[worldName].visitLocks -= 1;
            }
        }

        private void PathProofToggle(string location, int proofTotal)
        {
            Data data = MainWindow.data;
            //reminder: location is what report is for, not from
            //reminder: con = 1 | non = 10 | peace = 100

            //find grid path icons are in
            string worlthPath = location + "Path";
            Grid pathgrid = data.WorldsData[location].top.FindName(worlthPath) as Grid;
            //find each of the path icons
            Image top = pathgrid.FindName(worlthPath + "_Con") as Image;
            Image mid = pathgrid.FindName(worlthPath + "_Non") as Image;
            Image bot = pathgrid.FindName(worlthPath + "_Pea") as Image;

            //if total is 0 then world isn't a path to light,
            //so change the middle icon to a cross and return.
            if (proofTotal == 0) //no path to light
            {
                //TODO: set up custom images for the mini cross icon
                mid.Source = new BitmapImage(new Uri("Images/System/cross.png", UriKind.Relative));
                mid.Visibility = Visibility.Visible;
                return;
            }

            //iterate through each of the proof values, highest first,
            //then set visibility, found state, and subtract the proof value from the total.
            //(i can simplify this without the bools, but this is a bit cleaner looking i think)
            if (proofTotal >= 100) //peace
            {
                bot.Visibility = Visibility.Visible;
                proofTotal -= 100;
            }
            if (proofTotal >= 10) //nonexsitance
            {
                mid.Visibility = Visibility.Visible;
                proofTotal -= 10;
            }
            if (proofTotal == 1) //connection
            {
                top.Visibility = Visibility.Visible;
            }
            else if (proofTotal != 0)
            {
                window.SetHintText("Impossible Path Error! How are you seeing this?");
            }
        }
    
    }
}