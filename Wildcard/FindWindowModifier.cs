using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Task = System.Threading.Tasks.Task;

namespace Wildcard
{
    public class FindWindowModifier
    {
        private readonly Events _events;
        //private readonly FindEvents _findEvents;

        public FindWindowModifier(DTE2 dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _events = dte.Events;

            //_findEvents = _events.FindEvents;
            //_findEvents.FindDone += _events_FindDone;
        }

        //private void _events_FindDone(vsFindResult Result, bool Cancelled)
        //{
        //    throw new NotImplementedException();
        //}

        public async Task StartScanAsync(CancellationToken ct)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            try
            {
                while (true)
                {
                    foreach (System.Windows.Window w in Application.Current.Windows)
                    {
                        if (!w.GetType().Name.Contains("FloatingWindow")) //at the moment we support only floating window
                        {
                            continue;
                        }

                        var findDialogControl = w.GetRecursiveByType("NewFindDialogControl");
                        if (findDialogControl == null)
                        {
                            continue;
                        }

                        var findAllButton = findDialogControl.GetRecursiveByName("FindAll") as Button;
                        if (findAllButton == null)
                        {
                            continue;
                        }

                        var checkBoxes = new List<CheckBox>();
                        findDialogControl.GetRecursiveByType(ref checkBoxes);
                        if (checkBoxes.Count != 6)
                        {
                            continue;
                        }
                        
                        //var regexCheckBox = checkBoxes[2];
                        var regexCheckBox = (
                            from checkBox in checkBoxes
                            where checkBox.Visibility == Visibility.Visible && checkBox.ActualHeight >= double.Epsilon
                            let crd = checkBox.PointToScreen(new Point(0, 0))
                            orderby crd.Y
                            select checkBox).Skip(2).FirstOrDefault();
                        //var eq = ReferenceEquals(regexCheckBox, regexCheckBox2);

                        if (regexCheckBox == null)
                        {
                            continue;
                        }

                        //var regexpTextBlock = findDialogControl.GetRecursiveTextBlockByNameItsText("Us_e regular expressions");
                        //if (regexpTextBlock == null)
                        //{
                        //    continue;
                        //}

                        //var regexCheckBox = regexpTextBlock.Parent as CheckBox;
                        //if (regexCheckBox == null)
                        //{
                        //    continue;
                        //}

                        var comboBoxes = new List<ComboBox>();
                        findDialogControl.GetRecursiveByType(ref comboBoxes);
                        if (comboBoxes.Count != 4)
                        {
                            continue;
                        }

                        //var patternComboBox = comboBoxes[2];
                        var patternComboBox = (
                            from comboBox in comboBoxes
                            where comboBox.Visibility == Visibility.Visible && comboBox.ActualHeight >= double.Epsilon
                            let crd = comboBox.PointToScreen(new Point(0, 0))
                            orderby crd.Y 
                            select comboBox).FirstOrDefault();
                        //var eq = ReferenceEquals(patternComboBox, patternComboBox3);

                        if (patternComboBox == null)
                        {
                            continue;
                        }

                        //unsubscribe all default handlers
                        var routedEventHandlers = GetRoutedEventHandlers(findAllButton, ButtonBase.ClickEvent);
                        foreach (var routedEventHandler in routedEventHandlers)
                        {
                            findAllButton.Click -= (RoutedEventHandler)routedEventHandler.Handler;
                        }

                        findAllButton.BorderBrush = Brushes.Red;

                        (findAllButton.Content as AccessText).Unloaded += (sender, e) =>
                        {
                            if(OriginalState.WaitForUndo)
                            {
                                patternComboBox.Text = OriginalState.FindPattern ?? string.Empty;
                                regexCheckBox.IsChecked = OriginalState.IsRegexpCheckBoxChecked;
                                OriginalState.WaitForUndo = false;
                            }
                        };

                        //KeyBinding kb = new KeyBinding();
                        //kb.Modifiers |= ModifierKeys.Alt;
                        //kb.Key = Key.Z;
                        //kb.Command = new RelayCommand(
                        //    (s) =>
                        //    {
                        //        int g = 0;
                        //    });
                        //w.InputBindings.Add(kb);

                        //RoutedCommand firstSettings = new RoutedCommand();
                        //firstSettings.InputGestures.Add(new KeyGesture(Key.A, ModifierKeys.Alt));
                        //findDialogControl.CommandBindings.Add(new CommandBinding(firstSettings, (sender, e) =>
                        //{
                        //    int g = 0;
                        //}));
                        //patternComboBox.CommandBindings.Add(new CommandBinding(firstSettings, (sender, e) =>
                        //{
                        //    int g = 0;
                        //}));
                        //findAllButton.CommandBindings.Add(new CommandBinding(firstSettings, (sender, e) =>
                        //{
                        //    int g = 0;
                        //}));

                        //w.TextInput += (sender, e) =>
                        //{
                        //    if (findDialogControl.IsFocusedRecursive())
                        //    {
                        //        int g = 0;
                        //        //e.

                        //        //Debug.WriteLine("------------------------->          " + e.Key);
                        //        //if (e.Key == System.Windows.Input.Key.A)
                        //        //{
                        //        //    if (e.KeyboardDevice.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Alt))
                        //        //    {
                        //        //        e.Handled = true;
                        //        //    }
                        //        //}
                        //    }
                        //};
                        //w.PreviewKeyDown += (sender, e) =>
                        //{
                        //    if (e.KeyboardDevice.Modifiers == System.Windows.Input.ModifierKeys.Alt
                        //        )
                        //    {
                        //        if (e.Key == System.Windows.Input.Key.A)
                        //        {
                        //            int g = 0;
                        //        }
                        //    }
                        //    //if (findDialogControl.IsFocusedRecursive())
                        //    //{
                        //    //    Debug.WriteLine("------------------------->          " + e.Key);
                        //    //    if (e.Key == System.Windows.Input.Key.A)
                        //    //    {
                        //    //        if (e.KeyboardDevice.Modifiers.HasFlag(System.Windows.Input.ModifierKeys.Alt))
                        //    //        {
                        //    //            e.Handled = true;
                        //    //        }
                        //    //    }
                        //    //}
                        //};


                        //findAllButton.Content = "Find _All by w/c";

                        findAllButton.Click += (sender, e) =>
                        {
                            OriginalState.WaitForUndo = false;
                            if (!regexCheckBox.IsChecked.GetValueOrDefault(true))
                            {
                                //no regexp mode; regexp can contain ? and * so we does not enable wildcard mode if regexp checkbox is set
                                if (patternComboBox.Text.Contains("?") || patternComboBox.Text.Contains("*"))
                                {
                                    //process by wildcard
                                    OriginalState.FindPattern = patternComboBox.Text;
                                    OriginalState.IsRegexpCheckBoxChecked = regexCheckBox.IsChecked ?? false;
                                    OriginalState.WaitForUndo = true;

                                    var modifiedText = Regex.Escape(OriginalState.FindPattern)
                                        .Replace("\\*", ".*")
                                        .Replace("\\?", ".");
                                    ;

                                    regexCheckBox.IsChecked = true;
                                    patternComboBox.Text = modifiedText;
                                }
                            }

                            //imitation of the regular click
                            foreach (var routedEventHandler in routedEventHandlers)
                            {
                                routedEventHandler.Handler.DynamicInvoke(sender, e);
                            }
                        };

                        //we've done
                        return;
                    }

                    await Task.Delay(250, ct);

                    if (ct.IsCancellationRequested)
                    {
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                //ok
            }
            catch (Exception excp)
            {
                Logging.Log(excp);
            }
        }

        /// <summary>
        /// stackoverflow programming, yay!!!
        /// https://stackoverflow.com/questions/9434817/how-to-remove-all-click-event-handlers
        /// </summary>
        public static RoutedEventHandlerInfo[] GetRoutedEventHandlers(UIElement element, RoutedEvent routedEvent)
        {
            // Get the EventHandlersStore instance which holds event handlers for the specified element.
            // The EventHandlersStore class is declared as internal.
            var eventHandlersStoreProperty = typeof(UIElement).GetProperty(
                "EventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic);
            object eventHandlersStore = eventHandlersStoreProperty.GetValue(element, null);

            // Invoke the GetRoutedEventHandlers method on the EventHandlersStore instance 
            // for getting an array of the subscribed event handlers.
            var getRoutedEventHandlers = eventHandlersStore.GetType().GetMethod(
                "GetRoutedEventHandlers", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var routedEventHandlers = (RoutedEventHandlerInfo[])getRoutedEventHandlers.Invoke(
                eventHandlersStore, new object[] { routedEvent });

            return routedEventHandlers;
        }
    }
    
    public static class OriginalState
    {
        public static bool WaitForUndo
        {
            get;
            set;
        }

        public static bool IsRegexpCheckBoxChecked
        {
            get;
            set;
        }

        public static string FindPattern
        {
            get;
            set;
        }
    }
}
