using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Globalization;

namespace MazeLib
{
    [TypeConverterAttribute(typeof(EventItemConverter)), DescriptionAttribute("Event Item Support")]
    public class EventItem
    {
        public EventItem()
        {
        }

        public EventItem(long t,int inpType,int val1, int val2)
        {
            time = t;
            type = inpType;
            param1 = val1;
            param2 = val2;
        }

        private long time = 0;
        [Category("Misc")]
        [Description("Time of the event")]
        [ReadOnly(true)]
        public long Time
        {
            get { return time; }
            set { time = value; }
        }

        private int type = -1;
        [Category("Misc")]
        [Description("Type of the event")]
        [ReadOnly(true)]
        public int Type
        {
            get { return type; }
            set { type = value; }
        }

        private int param1 = -1;
        [Category("Misc")]
        [Description("")]
        [ReadOnly(true)]
        public int Param1
        {
            get { return param1; }
            set { param1 = value; }
        }

        private int param2 = -1;
        [Category("Misc")]
        [Description("")]
        [ReadOnly(true)]
        public int Param2
        {
            get { return param2; }
            set { param2 = value; }
        }

        public override string ToString()
        {
            if (type == 5)
            {
                if (param2 == 0)
                {
                    return "Highlight Event";
                }
                else
                {
                    return "Self-reported Rating";
                }
            }
            else if (type > 0)
                return "Misc Event";
            else
                return "Empty";
        }



    }

    
    public class EventItemConverter : ExpandableObjectConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            //true means show a combobox
            return true;
        }
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            //true will limit to list. false will show the list, but allow free-form entry
            return true;
        }

        //public override
        //System.ComponentModel.TypeConverter.StandardValuesCollection
        //GetStandardValues(ITypeDescriptorContext context)
        //{
        //    //List<EventItem> theArray = new List<EventItem>();
        //    ////theArray.Add(new Texture(null,null,0));
        //    //theArray.Add(null);
        //    //List<EventItem> list = MazeMaker.Maze.GetModels();
        //    //foreach (EventItem t in list)
        //    //{
        //    //    theArray.Add(t);
        //    //}
        //    //return new StandardValuesCollection(theArray.ToArray());
        //    //return new StandardValuesCollection(MazeMaker.Maze.GetModels().ToArray());
        //    //return new StandardValuesCollection(new string[] { "entry1", "entry2", "entry3" });
        //}

        public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
        {
            if (destinationType == typeof(EventItem))
                return true;

            return base.CanConvertTo(context, destinationType);
        }
        public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
        {
            if (sourceType == typeof(String))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }
        public override object ConvertTo(ITypeDescriptorContext context,
                               CultureInfo culture,
                               object value,
                               System.Type destinationType)
        {
            if (destinationType == typeof(System.String) &&
                 value is EventItem)
            {
                EventItem so = (EventItem)value;
                return so.ToString();
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
        //public override object ConvertFrom(ITypeDescriptorContext context,
        //                      CultureInfo culture, object value)
        //{
        //    if (value is string)
        //    {
        //        try
        //        {
        //            if (value == null || (string)value == "")
        //                return null;
        //            List<EventItem> cList = EventItem.GetModels();
        //            foreach (EventItem t in cList)
        //            {
        //                if (t.Name.CompareTo(value) == 0)
        //                    return t;
        //            }
        //        }
        //        catch
        //        {
        //            throw new ArgumentException("Can not convert '" + (string)value + "' to type StaticModel");
        //        }
        //        return value;
        //    }
        //    return base.ConvertFrom(context, culture, value);
        //}

    }
     
}
