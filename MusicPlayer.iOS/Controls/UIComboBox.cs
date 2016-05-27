//   Licensed to the Apache Software Foundation (ASF) under one
//        or more contributor license agreements.  See the NOTICE file
//        distributed with this work for additional information
//        regarding copyright ownership.  The ASF licenses this file
//        to you under the Apache License, Version 2.0 (the
//        "License"); you may not use this file except in compliance
//        with the License.  You may obtain a copy of the License at
// 
//          http://www.apache.org/licenses/LICENSE-2.0
// 
//        Unless required by applicable law or agreed to in writing,
//        software distributed under the License is distributed on an
//        "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//        KIND, either express or implied.  See the License for the
//        specific language governing permissions and limitations
//        under the License.

using System;
using MonoTouch;
using UIKit;
using Foundation;
using CoreGraphics;
using ObjCRuntime;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;


namespace MusicPlayer.iOS
{
	public class UIComboBox : UIButton
	{
		PickerView pickerView;
		public UIViewController ViewForPicker;
		public event EventHandler ValueChanged;
		public event EventHandler PickerClosed;
		public event EventHandler PickerShown;

		public UIComboBox() : this(new CGRect(0, 0, 250, 36))
		{
		}

		public UIComboBox(CGRect rect) : base(rect)
		{
			//this.BorderStyle = UITextBorderStyle.RoundedRect;
			this.BackgroundColor = UIColor.Clear;
			pickerView = new PickerView()
			{
				Frame = new CGRect(0,40,320,480),
			};
			this.TouchDown += delegate { ShowPicker(); };
//			pickerView.TintColor =Style.Colors.AccentColor.Value;

			//pickerView.BackgroundColor = UIColor.Black;
			pickerView.Opaque = true;
//			pickerView.BackgroundColor =Style.Current.ScreensDefaults.Cell.Background.Value;

			pickerView.IndexChanged += delegate
			{
				var oldValue = this.Text;
				this.Text = pickerView.StringValue;
				if (ValueChanged != null && oldValue != Text)
					ValueChanged(this, null);
			};
		}

		public string Text
		{
			get { return CurrentTitle; }
			set { SetTitle(value, UIControlState.Normal); }
		}

		public UIColor TextColor
		{
			get { return this.CurrentTitleColor; }
			set { this.SetTitleColor(value, UIControlState.Normal); }
		}

		public UITextAlignment TextAlignment
		{
			get
			{
				switch (this.HorizontalAlignment)
				{
					case UIControlContentHorizontalAlignment.Center:
						return UITextAlignment.Center;
					case UIControlContentHorizontalAlignment.Left:
						return UITextAlignment.Left;
					case UIControlContentHorizontalAlignment.Right:
						return UITextAlignment.Right;
					case UIControlContentHorizontalAlignment.Fill:
						return UITextAlignment.Justified;
				}
				return UITextAlignment.Left;
			}
			set
			{
				switch (value)
				{
					case UITextAlignment.Center:
						this.HorizontalAlignment = UIControlContentHorizontalAlignment.Center;
						return;
						;
					case UITextAlignment.Left:
						this.HorizontalAlignment = UIControlContentHorizontalAlignment.Left;
						return;
					case UITextAlignment.Right:
						this.HorizontalAlignment = UIControlContentHorizontalAlignment.Right;
						return;
					case UITextAlignment.Justified:
						this.HorizontalAlignment = UIControlContentHorizontalAlignment.Fill;
						return;
				}
			}
		}

		public override bool CanBecomeFirstResponder
		{
			get { return false; }
		}


		public object[] Items
		{
			get { return pickerView.Items; }
			set { pickerView.Items = value; }
		}

		public string DisplayMember
		{
			get { return pickerView.DisplayMember; }
			set { pickerView.DisplayMember = value; }
		}

		UIViewController vc;
		UIPopoverController pop;
		UIActionSheet sheet;

		void ShowInPopOver()
		{
			if (vc == null)
			{
				vc = new EqualizerPickerViewController()
				{
					PresetSelected = (preset) => SelectedItem = preset,
				};
				pop = new UIPopoverController(vc);
			}
			pop.PopoverContentSize = pickerView.Frame.Size;
			pop.PresentFromRect(Frame, this.Superview, UIPopoverArrowDirection.Up, true);
		}

		public void ShowPicker()
		{
			if (PickerShown != null)
				PickerShown(this, null);

			var tb = new UITextField(new CGRect(0, -100, 15, 25));
			this.Superview.AddSubview(tb);
			tb.BecomeFirstResponder();
			tb.ResignFirstResponder();
			tb.RemoveFromSuperview();
			tb = null;

			if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
			{
				ShowInPopOver();
				return;
			}
			var eqVC = new EqualizerPickerViewController()
			{
				PresetSelected = (preset) => SelectedItem = preset,
			};


			ViewForPicker.PresentViewController(new UINavigationController(eqVC), true, null);
		}

		bool isHiding;

		public void HidePicker()
		{
			if (PickerClosed != null)
				PickerClosed(this, null);
			if (sheet == null)
				return;
			sheet.DismissWithClickedButtonIndex(0, true);
			sheet = null;
			//isHiding = true;
			//var parentView = ViewForPicker ?? Superview;
			//var parentH = parentView.Frame.Height;

			//UIView.BeginAnimations("slidePickerOut");			
			//UIView.SetAnimationDuration(0.3);
			//UIView.SetAnimationDelegate(this);
			//UIView.SetAnimationDidStopSelector (new Selector ("fadeOutDidFinish"));
			//var frame = pickerView.Frame;
			//frame.Height = parentH;
			//pickerView.Frame = frame;
			//UIView.CommitAnimations();
		}

		public object SelectedItem
		{
			get { return pickerView.SelectedItem; }
			set
			{
				var oldValue = pickerView.StringValue;
				pickerView.SelectedItem = value;
				this.Text = pickerView.StringValue;

				if (ValueChanged != null && oldValue != Text)
					ValueChanged(this, null);
			}
		}

		public int SelectedIndex
		{
			get { return pickerView.SelectedIndex; }
			set
			{
				pickerView.SelectedIndex = value;
				this.Text = pickerView.StringValue;
			}
		}
	}

	public class PickerView : UIPickerView
	{
		public PickerView()
			: base(new CGRect(0, 40, 0, 440))
		{
			this.ShowSelectionIndicator = true;
			this.BackgroundColor = UIColor.Clear;
		}

		object[] items;

		public object[] Items
		{
			get { return items; }
			set
			{
				items = value;
				this.Model = new PickerData(this);
				if (IndexChanged != null)
					IndexChanged(this, null);
			}
		}

		public string DisplayMember { get; set; }

		int selectedIndex;

		public int SelectedIndex
		{
			get { return selectedIndex; }
			set
			{
				if (selectedIndex == value)
					return;
				selectedIndex = value;
				this.Select(selectedIndex, 0, true);
				if (IndexChanged != null)
					IndexChanged(this, null);
			}
		}

		public object SelectedItem
		{
			get
			{
				if (selectedIndex >= items.Length)
					return null;
				return items[SelectedIndex];
			}
			set
			{
				var index = Array.IndexOf<object>(items, value);
				if (index == -1 || index >= items.Length)
					return;
				SelectedIndex = index;
			}
		}


		public string StringValue
		{
			get
			{
				if (SelectedItem == null)
					return "";
				if (string.IsNullOrEmpty(DisplayMember))
					return SelectedItem.ToString();
				return GetPropertyValue(SelectedItem, DisplayMember);
			}
		}

		public event EventHandler IndexChanged;

		public static string GetPropertyValue(object inObject, string propertyName)
		{
			PropertyInfo[] props = inObject.GetType().GetProperties();
			PropertyInfo prop = props.FirstOrDefault(p => p.Name == propertyName);
			if (prop != null)
				return prop.GetValue(inObject, null).ToString();
			return "";
		}

		public static object[] GetPropertyArray(object inObject, string propertyName)
		{
			PropertyInfo[] props = inObject.GetType().GetProperties();
			PropertyInfo prop = props.FirstOrDefault(p => p.Name == propertyName);
			if (prop != null)
			{
				var currentObject = prop.GetValue(inObject, null);
				if (currentObject.GetType().GetGenericTypeDefinition() == typeof (List<>))
				{
					return (new ArrayList((IList) currentObject)).ToArray();
				}

				else if (currentObject is Array)
				{
					return (object[]) currentObject;
				}
				else
				{
					return new object[1];
				}
			}
			return new object[1];
		}
	}

	public class PickerData : UIPickerViewModel
	{
		PickerView Picker;

		public PickerData(PickerView picker)
		{
			Picker = picker;
		}

		public override UIView GetView(UIPickerView picker, nint row, nint component, UIView view)
		{
			var cell = view as UILabel;
			if (cell == null)
			{
				cell = new UILabel()
				{
//					TextColor = Style.Colors.AccentColor.Value, //Style.ScreensDefaults.Cell.MainFontColor.Value,
//					Font =Style.Current.ScreensDefaults.Cell.MainFont.Value,
					TextAlignment = UITextAlignment.Center,
					BackgroundColor = UIColor.Clear,
				};
			}
			cell.Text = GetTitle(picker, row, component);
			return cell;
		}

		public override nint GetComponentCount(UIPickerView uipv)
		{
			return (1);
		}

		public override nint GetRowsInComponent(UIPickerView uipv, nint comp)
		{
			//each component has its own count.
			int rows = Picker.Items.Length;
			return (rows);
		}

		public override string GetTitle(UIPickerView uipv, nint row, nint comp)
		{
			//each component would get its own title.

			var theObject = Picker.Items[row];
			if (string.IsNullOrEmpty(Picker.DisplayMember))
				return theObject.ToString();
			return PickerView.GetPropertyValue(theObject, Picker.DisplayMember);
		}


		public override void Selected(UIPickerView uipv, nint row, nint comp)
		{
			Picker.SelectedIndex = (int) row;
			//Picker.Select(row,comp,false);
		}

		public override nfloat GetComponentWidth(UIPickerView uipv, nint comp)
		{
			return (300f);
		}

		public override nfloat GetRowHeight(UIPickerView uipv, nint comp)
		{
			return (40f);
		}
	}
}