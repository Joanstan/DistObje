using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistObje
{
    class Program
    {
        public class DbContext : System.Data.Entity.DbContext
        {
            public virtual DbSet<ObjDistribuir> Objetivos { get; set; }

            protected override void OnModelCreating(DbModelBuilder modelBuilder)
            {
                modelBuilder.Entity<ObjDistribuir>().Property(x => x.Porcentaje).HasPrecision(18, 4);
            }
        }

        public class ObjDistribuir
        {
            public int Id { get; set; }
            public string Nombre { get; set; }

            public Decimal Monto { get; set; }
            public Decimal Porcentaje { get; set; }
        }

        private static readonly List<ObjDistribuir> _objetivos = new List<ObjDistribuir>
            {
                new ObjDistribuir() {Nombre = "Persona 0", Monto = 37777},

                new ObjDistribuir() {Nombre = "Persona 2", Monto = 26671},
                new ObjDistribuir() {Nombre = "Persona 3", Monto = 1776},
                new ObjDistribuir() {Nombre = "Persona 4", Monto = 2750},
                new ObjDistribuir() {Nombre = "Persona 5", Monto = 1913},
                new ObjDistribuir() {Nombre = "Persona 6", Monto = 1700},
                new ObjDistribuir() {Nombre = "Persona 7", Monto = 1100},
                new ObjDistribuir() {Nombre = "Persona 8", Monto = 5754},
                new ObjDistribuir() {Nombre = "Persona 9", Monto = 1502},
                new ObjDistribuir() {Nombre = "Persona 10", Monto = 1287},
                new ObjDistribuir() {Nombre = "Persona 11", Monto = 2131},
                new ObjDistribuir() {Nombre = "Persona 12", Monto = 107},
                new ObjDistribuir() {Nombre = "Persona 13", Monto = 1704},
                new ObjDistribuir() {Nombre = "Persona 14", Monto = 320},
                new ObjDistribuir() {Nombre = "Persona 15", Monto = 562},
                new ObjDistribuir() {Nombre = "Persona 16", Monto = 225},
                new ObjDistribuir() {Nombre = "Persona 17", Monto = 524},
                new ObjDistribuir() {Nombre = "Persona 18", Monto = 567},
                new ObjDistribuir() {Nombre = "Persona 19", Monto = 1053},
                new ObjDistribuir() {Nombre = "Persona 20", Monto = 32},
                new ObjDistribuir() {Nombre = "Persona 21", Monto = 614},
                new ObjDistribuir() {Nombre = "Persona 22", Monto = 1430},
                new ObjDistribuir() {Nombre = "Persona 23", Monto = 563},
                new ObjDistribuir() {Nombre = "Persona 24", Monto = 20484},
                new ObjDistribuir() {Nombre = "Persona 25", Monto = 202},
                new ObjDistribuir() {Nombre = "Persona 26", Monto = 290},
            };


        private static void Main(string[] args)
        {
            var totalElementos = _objetivos.Sum(a => a.Monto);

            foreach (var item in _objetivos)
            {
                item.Porcentaje =
                    Math.Round(
                        item.Monto/(totalElementos > 0 ? totalElementos : item.Monto), 4);
            }


            var p = new DbContext();
            p.Objetivos.RemoveRange(p.Objetivos.ToList());
            p.SaveChanges();
            if (!p.Objetivos.Any())
            {
                p.Objetivos.AddRange(_objetivos);
                p.SaveChanges();
            }
            Console.WriteLine("From Memory");
            Console.WriteLine("Total Elementos {0}", totalElementos);
            Console.WriteLine("Total Porcentaje {0}", _objetivos.Sum(a => a.Porcentaje));

            Console.WriteLine("From Db");
            Console.WriteLine("Total Elementos {0}", p.Objetivos.Sum(a => a.Monto));
            Console.WriteLine("Total Porcentaje {0}", p.Objetivos.Sum(a => a.Porcentaje));
            Console.WriteLine("Total Porcentaje {0}", DoubleConverter.ToExactString((Double)p.Objetivos.Sum(a => a.Porcentaje)));



            Console.ReadKey();
        }


        /// <summary>
        /// A class to allow the conversion of doubles to string representations of
        /// their exact decimal values. The implementation aims for readability over
        /// efficiency.
        /// </summary>
        public class DoubleConverter
        {
            /// <summary>
            /// Converts the given double to a string representation of its
            /// exact decimal value.
            /// </summary>
            /// <param name="d">The double to convert.</param>
            /// <returns>A string representation of the double's exact decimal value.</return>
            public static string ToExactString(double d)
            {
                if (double.IsPositiveInfinity(d))
                    return "+Infinity";
                if (double.IsNegativeInfinity(d))
                    return "-Infinity";
                if (double.IsNaN(d))
                    return "NaN";

                // Translate the double into sign, exponent and mantissa.
                long bits = BitConverter.DoubleToInt64Bits(d);
                // Note that the shift is sign-extended, hence the test against -1 not 1
                bool negative = (bits < 0);
                int exponent = (int)((bits >> 52) & 0x7ffL);
                long mantissa = bits & 0xfffffffffffffL;

                // Subnormal numbers; exponent is effectively one higher,
                // but there's no extra normalisation bit in the mantissa
                if (exponent == 0)
                {
                    exponent++;
                }
                // Normal numbers; leave exponent as it is but add extra
                // bit to the front of the mantissa
                else
                {
                    mantissa = mantissa | (1L << 52);
                }

                // Bias the exponent. It's actually biased by 1023, but we're
                // treating the mantissa as m.0 rather than 0.m, so we need
                // to subtract another 52 from it.
                exponent -= 1075;

                if (mantissa == 0)
                {
                    return "0";
                }

                /* Normalize */
                while ((mantissa & 1) == 0)
                {    /*  i.e., Mantissa is even */
                    mantissa >>= 1;
                    exponent++;
                }

                /// Construct a new decimal expansion with the mantissa
                ArbitraryDecimal ad = new ArbitraryDecimal(mantissa);

                // If the exponent is less than 0, we need to repeatedly
                // divide by 2 - which is the equivalent of multiplying
                // by 5 and dividing by 10.
                if (exponent < 0)
                {
                    for (int i = 0; i < -exponent; i++)
                        ad.MultiplyBy(5);
                    ad.Shift(-exponent);
                }
                // Otherwise, we need to repeatedly multiply by 2
                else
                {
                    for (int i = 0; i < exponent; i++)
                        ad.MultiplyBy(2);
                }

                // Finally, return the string with an appropriate sign
                if (negative)
                    return "-" + ad.ToString();
                else
                    return ad.ToString();
            }

            /// <summary>Private class used for manipulating
            class ArbitraryDecimal
            {
                /// <summary>Digits in the decimal expansion, one byte per digit
                byte[] digits;
                /// <summary> 
                /// How many digits are *after* the decimal point
                /// </summary>
                int decimalPoint = 0;

                /// <summary> 
                /// Constructs an arbitrary decimal expansion from the given long.
                /// The long must not be negative.
                /// </summary>
                internal ArbitraryDecimal(long x)
                {
                    string tmp = x.ToString(CultureInfo.InvariantCulture);
                    digits = new byte[tmp.Length];
                    for (int i = 0; i < tmp.Length; i++)
                        digits[i] = (byte)(tmp[i] - '0');
                    Normalize();
                }

                /// <summary>
                /// Multiplies the current expansion by the given amount, which should
                /// only be 2 or 5.
                /// </summary>
                internal void MultiplyBy(int amount)
                {
                    byte[] result = new byte[digits.Length + 1];
                    for (int i = digits.Length - 1; i >= 0; i--)
                    {
                        int resultDigit = digits[i] * amount + result[i + 1];
                        result[i] = (byte)(resultDigit / 10);
                        result[i + 1] = (byte)(resultDigit % 10);
                    }
                    if (result[0] != 0)
                    {
                        digits = result;
                    }
                    else
                    {
                        Array.Copy(result, 1, digits, 0, digits.Length);
                    }
                    Normalize();
                }

                /// <summary>
                /// Shifts the decimal point; a negative value makes
                /// the decimal expansion bigger (as fewer digits come after the
                /// decimal place) and a positive value makes the decimal
                /// expansion smaller.
                /// </summary>
                internal void Shift(int amount)
                {
                    decimalPoint += amount;
                }

                /// <summary>
                /// Removes leading/trailing zeroes from the expansion.
                /// </summary>
                internal void Normalize()
                {
                    int first;
                    for (first = 0; first < digits.Length; first++)
                        if (digits[first] != 0)
                            break;
                    int last;
                    for (last = digits.Length - 1; last >= 0; last--)
                        if (digits[last] != 0)
                            break;

                    if (first == 0 && last == digits.Length - 1)
                        return;

                    byte[] tmp = new byte[last - first + 1];
                    for (int i = 0; i < tmp.Length; i++)
                        tmp[i] = digits[i + first];

                    decimalPoint -= digits.Length - (last + 1);
                    digits = tmp;
                }

                /// <summary>
                /// Converts the value to a proper decimal string representation.
                /// </summary>
                public override String ToString()
                {
                    char[] digitString = new char[digits.Length];
                    for (int i = 0; i < digits.Length; i++)
                        digitString[i] = (char)(digits[i] + '0');

                    // Simplest case - nothing after the decimal point,
                    // and last real digit is non-zero, eg value=35
                    if (decimalPoint == 0)
                    {
                        return new string(digitString);
                    }

                    // Fairly simple case - nothing after the decimal
                    // point, but some 0s to add, eg value=350
                    if (decimalPoint < 0)
                    {
                        return new string(digitString) +
                               new string('0', -decimalPoint);
                    }

                    // Nothing before the decimal point, eg 0.035
                    if (decimalPoint >= digitString.Length)
                    {
                        return "0." +
                            new string('0', (decimalPoint - digitString.Length)) +
                            new string(digitString);
                    }

                    // Most complicated case - part of the string comes
                    // before the decimal point, part comes after it,
                    // eg 3.5
                    return new string(digitString, 0,
                                       digitString.Length - decimalPoint) +
                        "." +
                        new string(digitString,
                                    digitString.Length - decimalPoint,
                                    decimalPoint);
                }
            }
        }




        private static decimal RoundToSignificantDigits(decimal m, int digits)
        {
            // nothing to do if the value is zero  
            if (m == 0.0M) return m;
            var d = Convert.ToDouble(m);
            double scale = Math.Pow(10, Math.Floor(Math.Log10(d)) + 1);
            return Convert.ToDecimal(scale * Math.Round(d / scale, digits));
        }


    }
}
