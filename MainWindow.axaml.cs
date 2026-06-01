using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TMGSSaveEditor;

namespace TMGSSaveEditor.Core
{
    public partial class MainWindow : Window
    {
        public Object data;

        internal ISaveDataManager _savedata;

        bool hasLoaded => data != null;
        bool hasChanges;

        public class ObjectInfo
        {
            public Object obj;
            public Object parentObj;
            public FieldInfo fieldInfo;
            public int arrIndex;
            public TreeViewItem treeNode;
        }

        Dictionary<TreeViewItem, ObjectInfo> nodeToObjDict = new Dictionary<TreeViewItem, ObjectInfo>();
        ObservableCollection<TreeViewItem> treeItems = new ObservableCollection<TreeViewItem>();

        public delegate void objectInfoDelegate(ObjectInfo objInfo);
        public delegate void voidDelegate();

        public event objectInfoDelegate onHandleObject;
        public event voidDelegate onReset;

        private CheatsWindow _cheatsWindowInstance;

        private string _gameTitle;

        public interface ObjectInspector
        {
            void registerParent(MainWindow form);
        }

        private TreeViewItem CreateNodeWithLoading(string text)
        {
            TreeViewItem node = new TreeViewItem { Header = text };
            node.ItemsSource = new ObservableCollection<TreeViewItem> { new TreeViewItem { Header = "loading" } };
            return node;
        }

        public MainWindow()
        {
            InitializeComponent();
            TreeView1.ItemsSource = treeItems;
            TreeView1.AddHandler(TreeViewItem.ExpandedEvent, TreeViewItem_Expanded);
            this.Loaded += MainWindow_Loaded;
        }

        public MainWindow(ISaveDataManager saveDataManager, string projectName, string windowTitle) : this()
        {
            _savedata = saveDataManager;

            _gameTitle = windowTitle;

            this.Title = windowTitle;

            LoadGameAssets(projectName);
        }

        private void LoadGameAssets(string projectName)
        {
            if (string.IsNullOrEmpty(projectName)) return;

            try
            {
                string charPath = $"avares://{projectName}/Assets/character.png";
                string logoPath = $"avares://{projectName}/Assets/logo.png";

                PictureBoxCharacter.Source = new Bitmap(AssetLoader.Open(new Uri(charPath)));
                PictureBoxLogo.Source = new Bitmap(AssetLoader.Open(new Uri(logoPath)));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load dynamic images: {ex.Message}");
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Instantiate the new Avalonia UserControls
            List<UserControl> a = new List<UserControl> {
             new BooleanControl(),
             new TextControl(),
             new EnumControl(),
             new DateControl(),
    };

            foreach (UserControl x in a)
            {
                // Add them to the empty panel on the right side of the screen
                PanelEditors.Children.Add(x);

                // Hide them by default
                x.IsVisible = false;

                // Register the events
                if (x is ObjectInspector inspector)
                {
                    inspector.registerParent(this);
                }
            }
        }

        private async void ButtonLoad_Click(object sender, RoutedEventArgs e)
        {
            if (hasChanges)
            {
                // Requires MsBox.Avalonia NuGet package for MessageBox functionality, 
                // or a custom dialog implementation.
                // var result = await MessageBoxManager.GetMessageBoxStandard("Load anyway?", "File has been modified. Load anyway?", ButtonEnum.YesNo, Icon.Warning).ShowAsync();
                // if (result != ButtonResult.Yes) return;
            }

            var topLevel = TopLevel.GetTopLevel(this);
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Save File",
                AllowMultiple = false
            });

            if (files.Count == 0)
            {
                return;
            }

            string path = files[0].Path.LocalPath;

            data = _savedata.Load(path);

            nodeToObjDict.Clear();
            onReset?.Invoke();
            treeItems.Clear();

            TreeViewItem rootNode = CreateNodeWithLoading("Root");
            processRootNode(rootNode);
        }

        void saveTypeNode(TreeViewItem node, Object o, Object parentObj, FieldInfo fieldInfo, int arrIndex = -1)
        {
            nodeToObjDict[node] = new ObjectInfo
            {
                obj = o,
                parentObj = parentObj,
                fieldInfo = fieldInfo,
                arrIndex = arrIndex,
                treeNode = node,
            };
        }

        private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            // In Avalonia, e.Source tells us exactly which node was expanded
            if (e.Source is TreeViewItem node)
            {
                processNode(node);
            }
        }

        void processRootNode(TreeViewItem node)
        {
            if (data == null) return;
            Type rootType = data.GetType();

            node.Header = rootType.Name;
            treeItems.Add(node);

            saveTypeNode(node, data, data, rootType.GetFields().First());
        }

        void processNode(TreeViewItem parent)
        {
            if (!nodeToObjDict.ContainsKey(parent)) return;

            ObjectInfo objInfo = nodeToObjDict[parent];
            Object o = objInfo.obj;
            if (o == null) return;
            Type t = o.GetType();

            if (t.IsPrimitive || t == typeof(string) || t.IsEnum) return;

            // Grab the children collection attached to this parent
            ObservableCollection<TreeViewItem> childCollection = parent.ItemsSource as ObservableCollection<TreeViewItem>;
            if (childCollection == null)
            {
                childCollection = new ObservableCollection<TreeViewItem>();
                parent.ItemsSource = childCollection;
            }

            // Clear previous items or the "loading" dummy node
            childCollection.Clear();

            if (t.BaseType == typeof(Array))
            {
                Array arrayOfItems = (Array)o;
                int i = 0;
                foreach (var item in arrayOfItems)
                {
                    if (item == null) continue;
                    Type itemType = item.GetType();

                    string customSuffix = "";
                    bool hideTypeName = false;

                    if (objInfo.fieldInfo != null)
                    {
                        if (objInfo.fieldInfo.Name.Equals("approachChars", StringComparison.OrdinalIgnoreCase))
                        {
                            if (_savedata.ApproachCharacterNames != null && i < _savedata.ApproachCharacterNames.Length)
                            {
                                customSuffix = " - " + _savedata.ApproachCharacterNames[i];
                            }
                        }
                        else if (objInfo.fieldInfo.Name.Equals("friendChars", StringComparison.OrdinalIgnoreCase))
                        {
                            if (_savedata.FriendCharacterNames != null && i < _savedata.FriendCharacterNames.Length)
                            {
                                customSuffix = " - " + _savedata.FriendCharacterNames[i];
                            }
                        }
                        else if (objInfo.fieldInfo.Name.Equals("advChars", StringComparison.OrdinalIgnoreCase))
                        {
                            if (_savedata.AdvCharacterNames != null && i < _savedata.AdvCharacterNames.Length)
                            {
                                customSuffix = " - " + _savedata.AdvCharacterNames[i];
                            }
                        }
                        else if (objInfo.fieldInfo.Name.Equals("standardParams", StringComparison.OrdinalIgnoreCase))
                        {
                            if (_savedata.ParameterNames != null && i < _savedata.ParameterNames.Length)
                            {
                                customSuffix = " - " + _savedata.ParameterNames[i];
                            }
                        }
                        else if (objInfo.fieldInfo.Name.Equals("isPossessionDresses", StringComparison.OrdinalIgnoreCase))
                        {
                            if (_savedata.ClothingNames != null && i < _savedata.ClothingNames.Length)
                            {
                                // Remove the hyphen prefix so it looks clean
                                customSuffix = _savedata.ClothingNames[i];
                                hideTypeName = true;
                            }
                        }
                    }

                    string name;
                    TreeViewItem node;
                    if (itemType.IsPrimitive || itemType == typeof(string) || itemType.BaseType == typeof(Enum) || itemType.Namespace != "GS4")
                    {
                        string labelText = hideTypeName
                            ? String.Format("[{0}] {1}:", i, customSuffix)
                            : String.Format("[{0}] {1}{2}:", i, itemType.Name, customSuffix);

                        // 1. Create a horizontal panel to hold the text
                        StackPanel headerPanel = new StackPanel { Orientation = Avalonia.Layout.Orientation.Horizontal };

                        // 2. Create the label (the item name) with a fixed width to force alignment
                        TextBlock labelBlock = new TextBlock
                        {
                            Text = labelText,
                            Width = 450, // You can increase this if extremely long names get cut off
                            TextTrimming = Avalonia.Media.TextTrimming.CharacterEllipsis // Adds "..." if it exceeds the width
                        };

                        // 3. Create the value block (True/False)
                        TextBlock valueBlock = new TextBlock
                        {
                            Text = item.ToString()
                        };

                        // 4. Put them together
                        headerPanel.Children.Add(labelBlock);
                        headerPanel.Children.Add(valueBlock);

                        // 5. Assign the whole panel to the node header instead of a string
                        node = new TreeViewItem { Header = headerPanel };
                    }
                    else
                    {
                        // Keep the original logic for complex expanding nodes (like structs)
                        string nodeName = hideTypeName
                            ? String.Format("[{0}] {1}", i, customSuffix)
                            : String.Format("[{0}] {1}{2}", i, itemType.Name, customSuffix);

                        node = CreateNodeWithLoading(nodeName);
                    }

                    childCollection.Add(node);
                    saveTypeNode(node, item, o, objInfo.fieldInfo, i);
                    i++;
                }
                return;
            }

            foreach (FieldInfo x in t.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                Type childType = x.FieldType;
                Object childObject = x.GetValue(o);
                TreeViewItem childNode;

                if (childObject == null)
                {
                    childNode = new TreeViewItem { Header = String.Format("{0}: null", x.Name) };
                }
                else if (childType.Namespace != "GS4" && childType.BaseType?.Name != "Array" || childType.BaseType == typeof(Enum))
                {
                    childNode = new TreeViewItem { Header = String.Format("{0}: {1}", x.Name, childObject.ToString()) };
                }
                else
                {
                    childNode = CreateNodeWithLoading(x.Name);
                }

                childCollection.Add(childNode);
                saveTypeNode(childNode, childObject, o, x);
            }
        }

        private void TreeView1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If nothing is selected, do nothing
            if (TreeView1.SelectedItems == null || TreeView1.SelectedItems.Count == 0) return;

            // Base the right-side editor on the primary/first selected item
            if (TreeView1.SelectedItems[0] is not TreeViewItem node) return;

            onReset?.Invoke();

            ObservableCollection<TreeViewItem> children = node.ItemsSource as ObservableCollection<TreeViewItem>;
            if (children != null && children.Count > 0 && children[0].Header?.ToString() != "loading")
            {
                return;
            }

            if (nodeToObjDict.TryGetValue(node, out ObjectInfo o))
            {
                onHandleObject?.Invoke(o);
            }
        }

        public void setObject(ObjectInfo objInfo, Object newObject)
        {
            if (TreeView1.SelectedItems == null || TreeView1.SelectedItems.Count == 0) return;

            var selectedNodes = TreeView1.SelectedItems.Cast<TreeViewItem>().ToList();

            var signatures = selectedNodes.Select(node =>
            {
                var info = nodeToObjDict[node];
                return new { info.parentObj, info.fieldInfo, info.arrIndex };
            }).ToList();

            HashSet<TreeViewItem> parentsToRefresh = new HashSet<TreeViewItem>();

            foreach (var node in selectedNodes)
            {
                if (nodeToObjDict.TryGetValue(node, out ObjectInfo currentObjInfo))
                {
                    if (currentObjInfo.obj != null && currentObjInfo.obj.GetType() == newObject.GetType())
                    {
                        if (currentObjInfo.arrIndex != -1)
                        {
                            Array a = (Array)currentObjInfo.parentObj;
                            a.SetValue(newObject, currentObjInfo.arrIndex);
                        }
                        else
                        {
                            currentObjInfo.fieldInfo.SetValue(currentObjInfo.parentObj, newObject);
                        }

                        currentObjInfo.obj = newObject;

                        if (currentObjInfo.treeNode.Parent is TreeViewItem parentNode)
                        {
                            parentsToRefresh.Add(parentNode);
                        }
                    }
                }
            }

            hasChanges = true;

            // UNHOOK EVENT: Stop the TreeView from panicking while we rebuild the UI
            TreeView1.SelectionChanged -= TreeView1_SelectionChanged;

            foreach (var parent in parentsToRefresh)
            {
                if (parent.ItemsSource as ObservableCollection<TreeViewItem> is var oldChildren && oldChildren != null)
                {
                    foreach (var child in oldChildren)
                    {
                        nodeToObjDict.Remove(child);
                    }
                }
                processNode(parent);
            }

            TreeView1.SelectedItems.Clear();
            TreeViewItem primaryNodeToScrollTo = null;

            foreach (var sig in signatures)
            {
                TreeViewItem match = null;

                foreach (var parent in parentsToRefresh)
                {
                    if (parent.ItemsSource as ObservableCollection<TreeViewItem> is var newChildren && newChildren != null)
                    {
                        match = newChildren.FirstOrDefault(child =>
                            nodeToObjDict.TryGetValue(child, out var info) &&
                            info.parentObj == sig.parentObj &&
                            info.fieldInfo == sig.fieldInfo &&
                            info.arrIndex == sig.arrIndex);

                        if (match != null) break;
                    }
                }

                if (match != null)
                {
                    TreeView1.SelectedItems.Add(match);
                    if (primaryNodeToScrollTo == null) primaryNodeToScrollTo = match;
                }
            }

            // REHOOK EVENT: The tree is rebuilt, it is safe to listen to clicks again
            TreeView1.SelectionChanged += TreeView1_SelectionChanged;

            // Manually hand the fresh, newly generated node data directly to the TextControl
            if (primaryNodeToScrollTo != null && nodeToObjDict.TryGetValue(primaryNodeToScrollTo, out var freshInfo))
            {
                onReset?.Invoke();
                onHandleObject?.Invoke(freshInfo);
                primaryNodeToScrollTo.BringIntoView();
            }
        }

        private async void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (!hasLoaded)
            {
                return;
            }

            var topLevel = TopLevel.GetTopLevel(this);
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save TMGS File",
                DefaultExtension = "*.*"
            });

            if (file != null)
            {
                string path = file.Path.LocalPath;
                _savedata.Save(path, data);
                hasChanges = false;
            }
        }

        private async void PictureBoxCharacter_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var dialog = new AboutDialog(_gameTitle);
            await dialog.ShowDialog(this);
        }

        /*private async void PictureBoxCharacter_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(new MessageBoxStandardParams
            {
                ContentTitle = "TMGS 1 Save Editor",
                ContentMessage = @"          --- Credits ---

Programming:
    - PlasmaGrass
    - CISAC

Graphic Design:
    - euphonia.exe
",
                HyperLinkParams = new HyperLinkParams
                {
                    Text = "TMGS Fan Patch Discord",
                    Action = new Action(() =>
                    {
                        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        var url = "https://discord.gg/Kw6mRY96hY";
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            //https://stackoverflow.com/a/2796367/241446
                            using var proc = new Process { StartInfo = { UseShellExecute = true, FileName = url } };
                            proc.Start();

                            return;
                        }

                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        {
                            Process.Start("x-www-browser", url);
                            return;
                        }

                        if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                            throw new Exception("invalid url: " + url);
                        Process.Start("open", url);
                        return;
                    })
                },

                ButtonDefinitions = ButtonEnum.Ok,

                WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner,

                FontFamily = new FontFamily("avares://TMGSSaveEditor.Core/Assets/Fonts/DF-ChuButoMaruGothic-W7.ttf#DFMaruGothic-Bd")
            });

            await box.ShowWindowDialogAsync(this);
        }*/

        // Saves the header names from the root down to the selected item
        private List<string> GetSelectedNodePath()
        {
            List<string> path = new List<string>();

            if (TreeView1.SelectedItem is TreeViewItem current)
            {
                while (current != null)
                {
                    // Insert at the beginning so the list goes from Root -> Leaf
                    path.Insert(0, current.Header?.ToString() ?? "");
                    current = current.Parent as TreeViewItem;
                }
            }

            return path;
        }

        // Walks down the newly built tree and expands nodes matching the saved path
        private void RestoreSelectedNodePath(List<string> path)
        {
            if (path == null || path.Count == 0) return;

            ObservableCollection<TreeViewItem> currentLevel = treeItems;
            TreeViewItem lastNode = null;

            foreach (string step in path)
            {
                if (currentLevel == null) break;

                // Find the node in the current level that matches the header name
                TreeViewItem match = currentLevel.FirstOrDefault(x => x.Header?.ToString() == step);

                if (match != null)
                {
                    lastNode = match;
                    match.IsExpanded = true; // Visually open the folder
                    processNode(match);      // Force populate its children immediately so the next loop iteration can find them

                    currentLevel = match.ItemsSource as ObservableCollection<TreeViewItem>;
                }
                else
                {
                    break; // If a name changed or vanished, stop trying to dig deeper
                }
            }

            // Finally, highlight the target node
            if (lastNode != null)
            {
                TreeView1.SelectedItem = lastNode;
                lastNode.BringIntoView(); // Scrolls the TreeView down to the item automatically
            }
        }

        public void MarkAsChangedAndRefresh()
        {
            hasChanges = true;

            List<string> savedPath = GetSelectedNodePath();

            nodeToObjDict.Clear();
            onReset?.Invoke();
            treeItems.Clear();

            TreeViewItem rootNode = CreateNodeWithLoading("Root");
            processRootNode(rootNode);

            RestoreSelectedNodePath(savedPath);
        }

        private void ButtonOpenCheats_Click(object sender, RoutedEventArgs e)
        {
            if (_cheatsWindowInstance != null && _cheatsWindowInstance.IsVisible)
            {
                // Restore window if minimized
                if (_cheatsWindowInstance.WindowState == WindowState.Minimized)
                {
                    _cheatsWindowInstance.WindowState = WindowState.Normal;
                }

                // Bring window to front
                _cheatsWindowInstance.Activate();
                return;
            }

            // Create and show new instance
            _cheatsWindowInstance = new CheatsWindow(this);
            _cheatsWindowInstance.Show(this);
        }
    }
}