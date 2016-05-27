using System;
using System.Collections.Generic;
using System.Linq;
using MusicPlayer.Data;
using SQLite;
using SimpleDatabase;

namespace MusicPlayer.Models
{
	public class EqualizerPreset
	{
		public string GlobalId { get; set; }
		int id;

		[PrimaryKeyAttribute, AutoIncrement]
		public int Id
		{
			get { return id; }
			set
			{
				id = value;
				Values =
					Database.Main.GetObjects<EqualizerPresetValue>(new GroupInfo() {Filter = " EqualizerId = " + id, OrderBy = "Order"})
						.ToArray();
			}
		}

		public void Clone()
		{
			GlobalId = new Guid().ToString();
			id = 0;
			IsPreset = false;
		}

		public bool IsPreset { get; set; }

		[Ignore, Newtonsoft.Json.JsonIgnore]
		public double[] DoubleValues
		{
			set
			{
				List<EqualizerPresetValue> values = new List<EqualizerPresetValue>();
				for (int i = 0; i < value.Length; i++)
				{
					values.Add(new EqualizerPresetValue {EqualizerId = this.id, Value = value[i], Order = i});
				}
				Values = values.ToArray();
			}
		}

		public string Name { get; set; }

		[Ignore, Newtonsoft.Json.JsonIgnore]
		public EqualizerPresetValue[] Values { get; set; }

		public void Save()
		{
			if (string.IsNullOrEmpty(GlobalId))
				GlobalId = new Guid().ToString();
			if (Id > 0)
			{
				Database.Main.Update(this);
				Database.Main.UpdateAll(Values);
			}
			else
			{
				var presets = Values.ToArray();
				Database.Main.Insert(this);
				foreach (var value in presets)
				{
					value.EqualizerId = id;
					Database.Main.Insert(value);
				}
			}
		}

		public void Delete()
		{
			Database.Main.Delete(this);
			foreach (var preset in Values)
				Database.Main.Delete(preset);
		}
	}
}