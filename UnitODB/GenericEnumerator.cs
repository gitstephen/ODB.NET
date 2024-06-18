using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;

namespace UnitODB
{
	public class GenericEnumerator<T> : IOdbEnumerator<T>, IEnumerable<T>, IEnumerable, IDisposable
	{
		protected IDataReader dr;

		private bool disposed;

		public List<OdbColumn> Columns { get; private set; }

		public GenericEnumerator(IQuery query)
		{
			dr = query.Read();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
			{
				return;
			}

			if (disposing && dr != null)
			{
				if (!dr.IsClosed)
				{
					dr.Close();
				}

				dr.Dispose();
			}

			dr = null;
			disposed = true;
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		public IEnumerator<T> GetEnumerator()
		{
			Type type = typeof(T);
            Columns = new List<OdbColumn>();

            foreach (OdbColumn column in OdbMapping.GetColumns(type))
            {
                Columns.Add(column);
            }

            while (dr.Read())
			{
				object b = GetObject(type);
				yield return (T)b;
			}

			dr.Close();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public virtual object GetObject(Type type)
		{
			if (OdbType.IsNullable(type))
			{
				type = Nullable.GetUnderlyingType(type);
			}

			if (type.IsClass && type != OdbType.String)
			{
				return GetEntity(type);
			}

			return GetValue(0);
		}

		public virtual object GetEntity(Type type)
		{
			object uninitializedObject = FormatterServices.GetUninitializedObject(type);

			for (int i = 0; i < Columns.Count; i++)
			{
				if (!Columns[i].Attribute.IsForeignKey)
				{
					object value = GetValue(Columns[i].Name);
					Columns[i].SetValue(uninitializedObject, value);
				}
			}

			return uninitializedObject;
		}

		public virtual object GetValue(string name)
		{
			if (dr[name] != DBNull.Value)
			{
				return dr[name];
			}
			return null;
		}

		public virtual object GetValue(int i)
		{
			if (dr[i] != DBNull.Value)
			{
				return dr[i];
			}
			return null;
		}
	}

}
