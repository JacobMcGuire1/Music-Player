﻿#pragma checksum "E:\OneDrive\Documents\Visual Studio\source\repos\Music thing\Music thing\AlbumPage.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "8C743027CDB9B4FEA5282EE9761082B3"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Music_thing
{
    partial class AlbumPage : 
        global::Windows.UI.Xaml.Controls.Page, 
        global::Windows.UI.Xaml.Markup.IComponentConnector,
        global::Windows.UI.Xaml.Markup.IComponentConnector2
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private static class XamlBindingSetters
        {
            public static void Set_Windows_UI_Xaml_Controls_ItemsControl_ItemsSource(global::Windows.UI.Xaml.Controls.ItemsControl obj, global::System.Object value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = (global::System.Object) global::Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(global::System.Object), targetNullValue);
                }
                obj.ItemsSource = value;
            }
            public static void Set_Windows_UI_Xaml_FrameworkElement_Tag(global::Windows.UI.Xaml.FrameworkElement obj, global::System.Object value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = (global::System.Object) global::Windows.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(global::System.Object), targetNullValue);
                }
                obj.Tag = value;
            }
            public static void Set_Windows_UI_Xaml_Controls_TextBlock_Text(global::Windows.UI.Xaml.Controls.TextBlock obj, global::System.String value, string targetNullValue)
            {
                if (value == null && targetNullValue != null)
                {
                    value = targetNullValue;
                }
                obj.Text = value ?? global::System.String.Empty;
            }
        };

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private class AlbumPage_obj3_Bindings :
            global::Windows.UI.Xaml.IDataTemplateExtension,
            global::Windows.UI.Xaml.Markup.IDataTemplateComponent,
            global::Windows.UI.Xaml.Markup.IComponentConnector,
            IAlbumPage_Bindings
        {
            private global::Music_thing.Song dataRoot;
            private bool initialized = false;
            private const int NOT_PHASED = (1 << 31);
            private const int DATA_CHANGED = (1 << 30);
            private bool removedDataContextHandler = false;

            // Fields for each control that has bindings.
            private global::System.WeakReference obj3;
            private global::Windows.UI.Xaml.Controls.Button obj4;
            private global::Windows.UI.Xaml.Controls.Button obj5;
            private global::Windows.UI.Xaml.Controls.TextBlock obj6;
            private global::Windows.UI.Xaml.Controls.TextBlock obj7;
            private global::Windows.UI.Xaml.Controls.TextBlock obj8;
            private global::Windows.UI.Xaml.Controls.TextBlock obj9;
            private global::Windows.UI.Xaml.Controls.TextBlock obj10;
            private global::Windows.UI.Xaml.Controls.TextBlock obj11;

            public AlbumPage_obj3_Bindings()
            {
            }

            // IComponentConnector

            public void Connect(int connectionId, global::System.Object target)
            {
                switch(connectionId)
                {
                    case 3: // AlbumPage.xaml line 15
                        this.obj3 = new global::System.WeakReference((global::Windows.UI.Xaml.Controls.StackPanel)target);
                        break;
                    case 4: // AlbumPage.xaml line 16
                        this.obj4 = (global::Windows.UI.Xaml.Controls.Button)target;
                        break;
                    case 5: // AlbumPage.xaml line 17
                        this.obj5 = (global::Windows.UI.Xaml.Controls.Button)target;
                        break;
                    case 6: // AlbumPage.xaml line 18
                        this.obj6 = (global::Windows.UI.Xaml.Controls.TextBlock)target;
                        break;
                    case 7: // AlbumPage.xaml line 19
                        this.obj7 = (global::Windows.UI.Xaml.Controls.TextBlock)target;
                        break;
                    case 8: // AlbumPage.xaml line 20
                        this.obj8 = (global::Windows.UI.Xaml.Controls.TextBlock)target;
                        break;
                    case 9: // AlbumPage.xaml line 21
                        this.obj9 = (global::Windows.UI.Xaml.Controls.TextBlock)target;
                        break;
                    case 10: // AlbumPage.xaml line 22
                        this.obj10 = (global::Windows.UI.Xaml.Controls.TextBlock)target;
                        break;
                    case 11: // AlbumPage.xaml line 23
                        this.obj11 = (global::Windows.UI.Xaml.Controls.TextBlock)target;
                        break;
                    default:
                        break;
                }
            }

            public void DataContextChangedHandler(global::Windows.UI.Xaml.FrameworkElement sender, global::Windows.UI.Xaml.DataContextChangedEventArgs args)
            {
                 if (this.SetDataRoot(args.NewValue))
                 {
                    this.Update();
                 }
            }

            // IDataTemplateExtension

            public bool ProcessBinding(uint phase)
            {
                throw new global::System.NotImplementedException();
            }

            public int ProcessBindings(global::Windows.UI.Xaml.Controls.ContainerContentChangingEventArgs args)
            {
                int nextPhase = -1;
                ProcessBindings(args.Item, args.ItemIndex, (int)args.Phase, out nextPhase);
                return nextPhase;
            }

            public void ResetTemplate()
            {
                Recycle();
            }

            // IDataTemplateComponent

            public void ProcessBindings(global::System.Object item, int itemIndex, int phase, out int nextPhase)
            {
                nextPhase = -1;
                switch(phase)
                {
                    case 0:
                        nextPhase = -1;
                        this.SetDataRoot(item);
                        if (!removedDataContextHandler)
                        {
                            removedDataContextHandler = true;
                            (this.obj3.Target as global::Windows.UI.Xaml.Controls.StackPanel).DataContextChanged -= this.DataContextChangedHandler;
                        }
                        this.initialized = true;
                        break;
                }
                this.Update_((global::Music_thing.Song) item, 1 << phase);
            }

            public void Recycle()
            {
            }

            // IAlbumPage_Bindings

            public void Initialize()
            {
                if (!this.initialized)
                {
                    this.Update();
                }
            }
            
            public void Update()
            {
                this.Update_(this.dataRoot, NOT_PHASED);
                this.initialized = true;
            }

            public void StopTracking()
            {
            }

            public void DisconnectUnloadedObject(int connectionId)
            {
                throw new global::System.ArgumentException("No unloadable elements to disconnect.");
            }

            public bool SetDataRoot(global::System.Object newDataRoot)
            {
                if (newDataRoot != null)
                {
                    this.dataRoot = (global::Music_thing.Song)newDataRoot;
                    return true;
                }
                return false;
            }

            // Update methods for each path node used in binding steps.
            private void Update_(global::Music_thing.Song obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | (1 << 0))) != 0)
                    {
                        this.Update_id(obj.id, phase);
                        this.Update_Title(obj.Title, phase);
                        this.Update_Album(obj.Album, phase);
                        this.Update_Artist(obj.Artist, phase);
                        this.Update_Year(obj.Year, phase);
                        this.Update_Duration(obj.Duration, phase);
                    }
                }
            }
            private void Update_id(global::System.Int32 obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // AlbumPage.xaml line 16
                    XamlBindingSetters.Set_Windows_UI_Xaml_FrameworkElement_Tag(this.obj4, obj, null);
                    // AlbumPage.xaml line 17
                    XamlBindingSetters.Set_Windows_UI_Xaml_FrameworkElement_Tag(this.obj5, obj, null);
                    // AlbumPage.xaml line 18
                    XamlBindingSetters.Set_Windows_UI_Xaml_Controls_TextBlock_Text(this.obj6, obj.ToString(), null);
                }
            }
            private void Update_Title(global::System.String obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // AlbumPage.xaml line 19
                    XamlBindingSetters.Set_Windows_UI_Xaml_Controls_TextBlock_Text(this.obj7, obj, null);
                }
            }
            private void Update_Album(global::System.String obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // AlbumPage.xaml line 20
                    XamlBindingSetters.Set_Windows_UI_Xaml_Controls_TextBlock_Text(this.obj8, obj, null);
                }
            }
            private void Update_Artist(global::System.String obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // AlbumPage.xaml line 21
                    XamlBindingSetters.Set_Windows_UI_Xaml_Controls_TextBlock_Text(this.obj9, obj, null);
                }
            }
            private void Update_Year(global::System.UInt32 obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // AlbumPage.xaml line 22
                    XamlBindingSetters.Set_Windows_UI_Xaml_Controls_TextBlock_Text(this.obj10, obj.ToString(), null);
                }
            }
            private void Update_Duration(global::System.TimeSpan obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // AlbumPage.xaml line 23
                    XamlBindingSetters.Set_Windows_UI_Xaml_Controls_TextBlock_Text(this.obj11, obj.ToString(), null);
                }
            }
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        private class AlbumPage_obj1_Bindings :
            global::Windows.UI.Xaml.Markup.IComponentConnector,
            IAlbumPage_Bindings
        {
            private global::Music_thing.AlbumPage dataRoot;
            private bool initialized = false;
            private const int NOT_PHASED = (1 << 31);
            private const int DATA_CHANGED = (1 << 30);

            // Fields for each control that has bindings.
            private global::Windows.UI.Xaml.Controls.ListView obj2;

            public AlbumPage_obj1_Bindings()
            {
            }

            // IComponentConnector

            public void Connect(int connectionId, global::System.Object target)
            {
                switch(connectionId)
                {
                    case 2: // AlbumPage.xaml line 12
                        this.obj2 = (global::Windows.UI.Xaml.Controls.ListView)target;
                        break;
                    default:
                        break;
                }
            }

            // IAlbumPage_Bindings

            public void Initialize()
            {
                if (!this.initialized)
                {
                    this.Update();
                }
            }
            
            public void Update()
            {
                this.Update_(this.dataRoot, NOT_PHASED);
                this.initialized = true;
            }

            public void StopTracking()
            {
            }

            public void DisconnectUnloadedObject(int connectionId)
            {
                throw new global::System.ArgumentException("No unloadable elements to disconnect.");
            }

            public bool SetDataRoot(global::System.Object newDataRoot)
            {
                if (newDataRoot != null)
                {
                    this.dataRoot = (global::Music_thing.AlbumPage)newDataRoot;
                    return true;
                }
                return false;
            }

            public void Loading(global::Windows.UI.Xaml.FrameworkElement src, object data)
            {
                this.Initialize();
            }

            // Update methods for each path node used in binding steps.
            private void Update_(global::Music_thing.AlbumPage obj, int phase)
            {
                if (obj != null)
                {
                    if ((phase & (NOT_PHASED | (1 << 0))) != 0)
                    {
                        this.Update_Songs(obj.Songs, phase);
                    }
                }
            }
            private void Update_Songs(global::System.Collections.ObjectModel.ObservableCollection<global::Music_thing.Song> obj, int phase)
            {
                if ((phase & ((1 << 0) | NOT_PHASED )) != 0)
                {
                    // AlbumPage.xaml line 12
                    XamlBindingSetters.Set_Windows_UI_Xaml_Controls_ItemsControl_ItemsSource(this.obj2, obj, null);
                }
            }
        }
        /// <summary>
        /// Connect()
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void Connect(int connectionId, object target)
        {
            switch(connectionId)
            {
            case 2: // AlbumPage.xaml line 12
                {
                    this.ListViewSongs = (global::Windows.UI.Xaml.Controls.ListView)(target);
                }
                break;
            case 4: // AlbumPage.xaml line 16
                {
                    global::Windows.UI.Xaml.Controls.Button element4 = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)element4).Click += this.playButton_Click;
                }
                break;
            case 5: // AlbumPage.xaml line 17
                {
                    global::Windows.UI.Xaml.Controls.Button element5 = (global::Windows.UI.Xaml.Controls.Button)(target);
                    ((global::Windows.UI.Xaml.Controls.Button)element5).Click += this.addToPlaylistButton_Click;
                }
                break;
            default:
                break;
            }
            this._contentLoaded = true;
        }

        /// <summary>
        /// GetBindingConnector(int connectionId, object target)
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Windows.UI.Xaml.Build.Tasks"," 10.0.17.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::Windows.UI.Xaml.Markup.IComponentConnector GetBindingConnector(int connectionId, object target)
        {
            global::Windows.UI.Xaml.Markup.IComponentConnector returnValue = null;
            switch(connectionId)
            {
            case 1: // AlbumPage.xaml line 1
                {                    
                    global::Windows.UI.Xaml.Controls.Page element1 = (global::Windows.UI.Xaml.Controls.Page)target;
                    AlbumPage_obj1_Bindings bindings = new AlbumPage_obj1_Bindings();
                    returnValue = bindings;
                    bindings.SetDataRoot(this);
                    this.Bindings = bindings;
                    element1.Loading += bindings.Loading;
                }
                break;
            case 3: // AlbumPage.xaml line 15
                {                    
                    global::Windows.UI.Xaml.Controls.StackPanel element3 = (global::Windows.UI.Xaml.Controls.StackPanel)target;
                    AlbumPage_obj3_Bindings bindings = new AlbumPage_obj3_Bindings();
                    returnValue = bindings;
                    bindings.SetDataRoot(element3.DataContext);
                    element3.DataContextChanged += bindings.DataContextChangedHandler;
                    global::Windows.UI.Xaml.DataTemplate.SetExtensionInstance(element3, bindings);
                    global::Windows.UI.Xaml.Markup.XamlBindingHelper.SetDataTemplateComponent(element3, bindings);
                }
                break;
            }
            return returnValue;
        }
    }
}
