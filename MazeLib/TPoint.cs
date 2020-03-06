#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Globalization;

#endregion

namespace MazeMaker
{
    [TypeConverterAttribute(typeof(TPointConverter)), DescriptionAttribute("Association of Texture in Maze Coordinates"),Serializable]
    public class TPoint
    {
        public TPoint()
        {

        }

        public TPoint(TPoint inp)
        {
            X = inp.X;
            Y = inp.Y;
        }
        public TPoint(double inpX,double inpY)
        {
            X = inpX;
            Y = inpY;
        }

        private double x = 0;
        [Category("Texture")]
        [Description("X Coordinate")]
        public double X
        {
            get { return x; }
            set { x = value; }
        }

        private double y = 0;
        [Category("Texture")]
        [Description("Y Coordinate")]
        public double Y
        {
            get { return y; }
            set { y = value; }
        }

    }

    public class TPointConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
        {
            if (destinationType == typeof(TPoint))
                return true;

            return base.CanConvertTo(context, destinationType);
        }
        public override bool CanConvertFrom(ITypeDescriptorContext context,
                              System.Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }
        public override object ConvertTo(ITypeDescriptorContext context,
                               CultureInfo culture,
                               object value,
                               System.Type destinationType)
        {
            if (destinationType == typeof(System.String) &&
                 value is TPoint)
            {

                TPoint so = (TPoint)value;
                return so.X.ToString(".##;-.##;0") + ", " + so.Y.ToString(".##;-.##;0");
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
        public override object ConvertFrom(ITypeDescriptorContext context,
                              CultureInfo culture, object value)
        {
            if (value is string)
            {
                try
                {
                    string s = (string)value;
                    int comma = s.IndexOf(',');
                    if (comma!=-1)
                    {
                        string strX = s.Substring(0, comma);
                        string strY = s.Substring(comma+1);
                        TPoint so = new TPoint();
                        so.X = double.Parse(strX);
                        so.Y = double.Parse(strY);
                        return so;
                    }
                }
                catch
                {
                    throw new ArgumentException("Can not convert '" + (string)value + "' to type TPoint");
                }


            }
            return base.ConvertFrom(context, culture, value);
        }

    }
}
