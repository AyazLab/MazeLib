#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;

#endregion

namespace MazeMaker
{
    [TypeConverterAttribute(typeof(MPointConverter)),DescriptionAttribute("Location in Maze Coordinates"),Serializable]
    public class MPoint
    {
        public MPoint()
        {

        }

        public MPoint(double inp_x,double inp_z)
        {
            x=inp_x;
            z=inp_z;
        }

        public MPoint(double inp_x, double inp_y,double inp_z)
        {
            x = inp_x;
            y = inp_y;
            z = inp_z;
        }

        public MPoint(MPoint inp)
        {
            x = inp.x;
            y = inp.y;
            z = inp.z;
        }

        public System.Drawing.PointF GetPointF()
        {
            return new System.Drawing.PointF((float)x,(float) z);
        }

        public void SetPointF(System.Drawing.PointF inp)
        {
            x = (double) inp.X;
            z = (double) inp.Y;
        }

        private double x = 0;
        [Category("Location")]
        [Description("X Coordinate")]
        public double X
        {
            get { return x; }
            set { x = value; }
        }

        private double y = 0;
        [Category("Location")]
        [Description("Y Coordinate")]
        public double Y
        {
            get { return y; }
            set { y = value; }
        }

        private double z = 0;
        [Category("Location")]
        [Description("Z Coordinate")]
        public double Z
        {
            get { return z; }
            set { z = value; }
        }
        public bool IsEqual(MPoint inp)
        {
            if (AreEqual(inp.X, x) && AreEqual(inp.Y, y) && AreEqual(inp.Z, z))
            {
                return true;
            }
            return false;
        }
        private bool AreEqual(double inp1, double inp2)
        {
            if (Math.Abs(inp1 - inp2) < 0.001)
                return true;
            else
                return false;
        }

        public static MPoint operator -(MPoint a, MPoint b)
        {
            return new MPoint(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static MPoint operator +(MPoint a, MPoint b)
        {
            return new MPoint(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public double GetDistance(MPoint a)
        {
            return Math.Sqrt(Math.Pow((a.X - this.X), 2) + Math.Pow((a.Y - this.Y), 2) + Math.Pow((a.Z - this.Z), 2));
        }


        public static bool operator ==(MPoint a, MPoint b)
        {
            return (a.X == b.X && a.Y == b.Y && a.z == b.Z);
        }
        public static bool operator !=(MPoint a, MPoint b)
        {
            return (a.X != b.X || a.Y != b.Y || a.z != b.Z);
        }

    }


    public class MPointConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
        {
            if (destinationType == typeof(MPoint))
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
                 value is MPoint)
            {

                MPoint so = (MPoint)value;

                return so.X.ToString(".##;-.##;0") + ", " + so.Y.ToString(".##;-.##;0") + ", " + so.Z.ToString(".##;-.##;0");
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
                    int comma1 = s.IndexOf(',');
                    int comma2 = s.IndexOf(',',comma1+1);
                    if (comma1 != -1 && comma2!= -1)
                    {
                        string strX = s.Substring(0, comma1);
                        string strY = s.Substring(comma1+1, comma2-comma1-1);
                        string strZ = s.Substring(comma2+1);
                        MPoint so = new MPoint();
                        so.X = double.Parse(strX);
                        so.Y = double.Parse(strY);
                        so.Z = double.Parse(strZ);                        
                        return so;
                    }
                }
                catch
                {
                    throw new ArgumentException("Can not convert '" + (string)value + "' to type MPoint");
                }


            }
            return base.ConvertFrom(context, culture, value);
        }

    }

}
