using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace DBMLUpdate
{
    /// <summary>
    /// Helper class turns row values into the correct object types
    /// </summary>
    internal class RowRead
    {
        private DataRow m_row;

        public RowRead(DataRow row)
        {
            m_row = row;
        }

        /// <summary>
        /// Converts a row value to a boolean
        /// </summary>
        /// <param name="column">The column the value is on</param>
        public bool ToBool(string column) { return ToType<bool>(column, false); }

        /// <summary>
        /// Converts a row value to a boolean
        /// </summary>
        /// <param name="column">The column the value is on</param>
        /// <param name="defaultValue">The default value to use if the value doesn't exist or is null</param>
        public bool ToBool(string column, bool defaultValue) { return ToType<bool>(column, defaultValue); }

        /// <summary>
        /// Converts a row value to a nullable boolean
        /// </summary>
        /// <param name="column">The column the value is on</param>
        /// <returns>Nullable(Of Bool)</returns>
        public bool? ToBoolNull(string column)
        {
            return ToNullable<bool>(column);
        }

        /// <summary>
        /// Converts a row value to a byte
        /// </summary>
        /// <param name="column">The column the value is on</param>
        /// <param name="defaultValue">The default value to use if the value doesn't exist or is null</param>
        /// <returns>Byte</returns>
        public byte ToByte(string column, byte defaultValue)
        {
            return ToType<byte>(column, defaultValue);
        }

        /// <summary>
        /// Converts a row to the nullable byte
        /// </summary>
        /// <param name="column">The Column the value is in</param>
        /// <returns>Byte?</returns>
        public byte? ToByteNull(string column)
        {
            return ToNullable<byte>(column);
        }

        /// <summary>
        /// Converts a row value to a byte array
        /// </summary>
        /// <param name="column">The column the value is on</param>
        /// <param name="defaultValue">The default value to use if column doesn't exist or is null</param>
        /// <returns>Byte()</returns>
        public byte[] ToBytes(string column, byte[] defaultValue)
        {
            return ToType<byte[]>(column, defaultValue);
        }

        /// <summary>
        /// Converts a row value to a DateTime
        /// </summary>
        /// <param name="column">The column the value is on</param>
        /// <param name="defaultValue">The default value to use if the value doesn't exist or is null</param>
        /// <returns>DateTime</returns>
        public DateTime ToDateTime(string column, DateTime defaultValue)
        {
            return ToType<DateTime>(column, defaultValue);
        }

        /// <summary>
        /// Converts a row value to a nullable datetime
        /// </summary>
        /// <param name="column">The column the value is on</param>
        /// <returns>DateTime?</returns>
        public DateTime? ToDateTimeNull(string column)
        {
            return ToNullable<DateTime>(column);
        }

        /// <summary>
        /// Converts a row value to a double
        /// </summary>
        /// <param name="column">The column the value is on</param>
        /// <param name="defaultValue">The default value to use if the value doesn't exist or is null</param>
        /// <returns>Double</returns>
        public double ToDbl(string column, double defaultValue)
        {
            return ToType<double>(column, defaultValue);
        }

        /// <summary>
        /// Converts a row to the nullable double
        /// </summary>
        /// <param name="column">The Column the value is in</param>
        /// <returns>Double?</returns>
        public double? ToDblNull(string column)
        {
            return ToNullable<double>(column);
        }

        /// <summary>
        /// Converts a row value to a decimal
        /// </summary>
        /// <param name="column">The column the value is on</param>
        /// <param name="defaultValue">The default value to use if the value doesn't exist or is null</param>
        /// <returns>Decimal</returns>
        public decimal ToDec(string column, decimal defaultValue)
        {
            return ToType<decimal>(column, defaultValue);
        }

        /// <summary>
        /// Converts a row valut to a nullable decimal
        /// </summary>
        /// <param name="column">The column the value is on</param>
        /// <returns>Decimal?</returns>
        public decimal? ToDecNull(string column)
        {
            return ToNullable<decimal>(column);
        }

        /// <summary>
        /// Converts a row value to a guid
        /// </summary>
        /// <param name="column">The column the value is on</param>
        /// <returns>Guid</returns>
        public Guid ToGUID(string column)
        {
            return ToType<Guid>(column, Guid.Empty);
        }

        /// <summary>
        /// Converts a row value to a nullable guid
        /// </summary>
        /// <param name="column">The column the value is on</param>
        /// <returns>Guid?</returns>
        public Guid? ToGUIDNull(string column)
        {
            return ToNullable<Guid>(column);
        }

        /// <summary>
        /// Converts a row value to an integer
        /// </summary>
        /// <param name="column">The column the value is on</param>
        public int ToInt(string column) { return ToType<int>(column, 0); }

        /// <summary>
        /// Converts a row value to an integer
        /// </summary>
        /// <param name="column">The column the value is on</param>
        /// <param name="defaultValue">The default value to use if the value doesn't exist or is null</param>
        public int ToInt(string column, int defaultValue) { return ToType<int>(column, defaultValue); }

        /// <summary>
        /// Converts a row to the nullable integer
        /// </summary>
        /// <param name="column">The Column the value is in</param>
        /// <returns>Integer?</returns>
        public int? ToIntNull(string column)
        {
            return ToNullable<int>(column);
        }

        /// <summary>
        /// Converts a row value to a string
        /// </summary>
        /// <param name="column">The column the value is on</param>
        public string ToStr(string column) { return ToType<string>(column, null); }
        /// <summary>
        /// Converts a row value to a string
        /// </summary>
        /// <param name="column">The column the value is on</param>
        /// <param name="defaultValue">The default value to use if the value doesn't exist or is null</param>
        public string ToStr(string column, string defaultValue) { return ToType<string>(column, defaultValue); }

        /// <summary>
        /// Converts a row value to a TimeSpan
        /// </summary>
        /// <param name="column">The column the value is in</param>
        /// <returns>TimeSpan</returns>
        public TimeSpan ToTimeSpan(string column)
        {
            return ToTimeSpan(column, TimeSpan.Zero);
        }

        /// <summary>
        /// Converts a row value to a TimeSpan
        /// </summary>
        /// <param name="column">The column the value is in</param>
        /// <param name="defaultValue">The default value to use of the value doesn't exist or is null</param>
        /// <returns>TimeSpan</returns>
        public TimeSpan ToTimeSpan(string column, TimeSpan defaultValue)
        {
            DateTime? dt = ToNullable<DateTime>(column);
            if(dt != null && dt.HasValue)
            {
                return dt.Value.TimeOfDay;
            }

            return defaultValue;
        }

        /// <summary>
        /// Converts a row value to a nullable timespan
        /// </summary>
        /// <param name="column">The column the value is on</param>
        /// <returns>TimeSpan?</returns>
        public TimeSpan? ToTimeSpanNull(string column)
        {
            DateTime? dt = ToNullable<DateTime>(column);
            if(dt != null && dt.HasValue)
            {
                return dt.Value.TimeOfDay;
            }

            return null;
        }

        /// <summary>
        /// Converts a row value to a Type T
        /// </summary>
        /// <param name="column">The column the value is on</param>
        /// <param name="defaultValue">The default value to use if the value doesn't exist or is null</param>
        /// <returns>T</returns>
        public T ToType<T>(string column, T defaultValue)
        {
            if(m_row.Table.Columns.Contains(column) && m_row[column] != DBNull.Value)
            {
                if(m_row[column] is T)
                {
                    return (T)m_row[column];
                }
                else if(typeof(T).IsEnum && (m_row[column] is int))
                {
                    return (T)m_row[column];
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// Converts a row value to a type Nullable(Of T)
        /// </summary>
        /// <param name="column">The column the value is on</param>
        /// <returns>T</returns>
        public T? ToNullable<T>(string column) where T : struct
        {
            if(m_row.Table.Columns.Contains(column) && m_row[column] != DBNull.Value)
            {
                if(m_row[column] is T)
                {
                    return (T)m_row[column];
                }
                else if(typeof(T).IsEnum && (m_row[column] is int || m_row[column] is byte))
                {
                    return (T)m_row[column];
                }

            }

            return null;
        }
    }
}
