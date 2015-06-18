using System;
using System.Data;
using System.Text;
using System.Collections.Generic;
 
 
namespace com.data
{
	//防止資料隱碼用
	public class SafeSQL
	{
		public static string Quote(string strData)
		{
			return string.Format("'{0}'", strData.Replace("'", "''"));
		}
 
		public static string QuoteLike(string strData)
		{
			return string.Format("'%{0}%'", strData.Replace("'", "''"));
		}
 
		public static string NoQuote(string strData)
		{
			return strData.Replace("'", "''");
		}
	}
 
	//資料庫模式
	public enum SqlServerType
	{
		MsSQL,
		MsSQL2005,
		MySQL,
		Oracle,
		DB2,
		PostgreSQL
	}
 
	public enum JoinType
	{
		Inner,
		Outer,
		Left,
		Right
	}
 
	public enum SqlWorkMode
	{
		Select,
		Insert,
		Update,
		Delete
	}
 
	public class SqlJoinData
	{
		private JoinType _JoinType;				//結合方式
		public string OrderType
		{
			get
			{
				switch (this._JoinType)
				{
					case JoinType.Inner:
						return "inner join";
					case JoinType.Outer:
						return "outer join";
					case JoinType.Left:
						return "left join";
					case JoinType.Right:
						return "right join";
				}
				return "";
			}
		}
		public string strFromTable;				//來源資料表
		public bool IsUsingMode;				//結合模式(true = using模式, false = on 模式)
		public string strUsingOrOn;				//結合條件, 或用來結合的欄位
 
		public SqlJoinData(JoinType _type, string _strFromTable, bool _IsUsingMode, string _strUsingOrOn)
		{
			this._JoinType = _type;
			this.strFromTable = _strFromTable;
			this.IsUsingMode = _IsUsingMode;
			this.strUsingOrOn = _strUsingOrOn;
		}
	}
 
 
	public class SqlOrderData
	{
		public SqlOrderData(string _strField, bool _IsOrderASC)
		{
			strField = _strField;
			IsOrderASC = _IsOrderASC;
		}
 
		public SqlOrderData(string _strField, string strOrderType)
		{
			strField = _strField;
			OrderType = strOrderType;
		}
 
		private string strField;
		private bool IsOrderASC;
 
		public string OrderType
		{
			get
			{
				return (IsOrderASC == true) ? "asc" : "desc";
			}
			set
			{
				IsOrderASC = (value.ToLower() == "asc") ? true : false;
			}
		}
 
 
		//反向排序用(用於select分頁時)
		public string OrderTypeInvert
		{
			get
			{
				return (IsOrderASC == false) ? "asc" : "desc";
			}
		}
 
		public bool IsASC
		{
			get
			{
				return IsOrderASC;
			}
			set
			{
				IsOrderASC = value;
			}
		}
 
		public bool IsDESC
		{
			get
			{
				return !IsOrderASC;
			}
			set
			{
				IsOrderASC = !value;
			}
		}
 
		public string Field
		{
			get
			{
				return strField;
			}
			set
			{
				strField = value;
			}
		}
	}
 
	/// <summary>
	/// SqlStringBuilder 的摘要描述
	/// </summary>
	public class SqlStringBuilder
	{
		public SqlStringBuilder()
		{
			//
			// TODO: 在此加入建構函式的程式碼
			//
		}
 
		public SqlServerType ServerType = SqlServerType.MsSQL;
		StringBuilder objSB = new StringBuilder();
		SqlWorkMode _WorkMode = SqlWorkMode.Select;
		string strSql_Original = null;
 
		bool IsDistinct = false;
		List<string> listQueryFields = new List<string>();
		string strIntoTable = null;
		List<string> listFrom = new List<string>();
		//Dictionary<string, string> setQueryFields = new Dictionary<string, string>();
		List<SqlJoinData> listJoin = new List<SqlJoinData>();
		List<string> listWhere = new List<string>();
		List<string> listGroup = new List<string>();
		List<string> listHaving = new List<string>();
		List<SqlOrderData> listOrder = new List<SqlOrderData>();
 
		int nCount = 0;
		int nOffset = 1;
 
		void AppendOrderString(bool IsInvertOrder)
		{	
			this.objSB.Append(" order by ");
			string strComma = "";
 
			if (IsInvertOrder == false)
			{
				//正向Order
				foreach (SqlOrderData objOrderData in this.listOrder)
				{
					objSB.Append(strComma).AppendFormat("{0} {1}", objOrderData.Field, objOrderData.OrderType);
					strComma = ",";
				}
			}
			else
			{
				//反向Order
				foreach (SqlOrderData objOrderData in this.listOrder)
				{
					this.objSB.Append(strComma).AppendFormat("{0} {1}", objOrderData.Field, objOrderData.OrderTypeInvert);
					strComma = ",";
				}
			}
		}
 
 
		public override string ToString()
		{
			//TODO: string ToString()
 
			#region "SQL字串組合 - select相關"
			if (this._WorkMode == SqlWorkMode.Select)
			{
				string strComma = "";
				this.objSB.Length = 0;
				this.objSB.Append("select");
 
				if (this.IsDistinct == true)
					this.objSB.Append(" distinct");
 
				if (this.nCount < 0)
				{
					throw new Exception("SqlStringBuilder不支援(Count小於0)的分頁功能.");
				}
				else if (this.nCount > 0)
				{
					if (this.nOffset == 1)
					{
						if ((this.ServerType == SqlServerType.MsSQL) ||
							(this.ServerType == SqlServerType.MsSQL2005))
						{
							//只有MsSQL或MsSQL2005才有top關鍵字
							this.objSB.AppendFormat(" top {0}", this.nCount);
						}
					}
					else if (this.nOffset < 1)
					{
						throw new Exception("SqlStringBuilder不支援(Offset小於1)的分頁功能.");
					}
					else
					{
						//還不支援(Offset非0的部份)
						//throw new Exception("SqlStringBuilder還不支援(Offset大於0)的分頁功能.");
 
						if (this.listOrder.Count == 0)
						{
							throw new Exception("SqlStringBuilder(分頁處理), 需要有排序的條件式.");
						}
						else if ((this.listOrder.Count > 0) &&
							(this.ServerType == SqlServerType.MsSQL2005))
						{
							//符合 MS-SQL 2005, 又啟用分頁功能時, 且具有order子句時, 不使用top
						}
						else if (this.ServerType == SqlServerType.MsSQL)
						{
							this.objSB.AppendFormat(" top {0}", (this.nOffset + this.nCount - 1));
						}
					}
				}
 
				//listQueryFields
				if (this.listQueryFields.Count != 0)
				{
					this.objSB.Append(" ");
					foreach (string strQueryFields in this.listQueryFields)
					{
						this.objSB.Append(strComma).Append(strQueryFields);
						strComma = ",";
					}
				}
				else
				{
					this.objSB.Append(" *");
				}
 
				//strIntoTable
				if (strIntoTable != null)
				{
					this.objSB.AppendFormat(" into {0}", strIntoTable);
				}
 
				//listFrom
				if (this.listFrom.Count == 0)
				{
					//錯誤(無資料來源)
					throw new Exception("SqlStringBuilder必須要有一個以上的資料來源.");
				}
				else
				{
					//1個以上的Table來源
					this.objSB.Append(" from ");
					strComma = "";
					foreach (string strFrom in this.listFrom)
					{
						this.objSB.Append(strComma).Append(strFrom);
						strComma = ",";
					}
				}
 
				//listJoin
				if (this.listJoin.Count != 0)
				{
					foreach (SqlJoinData objJoinData in this.listJoin)
					{
						this.objSB.AppendFormat(
							" {0} {1}",
							objJoinData.OrderType,
							objJoinData.strFromTable);
 
						if (objJoinData.IsUsingMode == false)
						{
							if (objJoinData.strUsingOrOn != null)
								this.objSB.AppendFormat(
									" on {0}",
									objJoinData.strUsingOrOn);
						}
						else
						{
 
							if (objJoinData.strUsingOrOn != null)
								this.objSB.AppendFormat(
									" using ({0})",
									objJoinData.strUsingOrOn);
						}
					}
				}
 
				//listWhere
				if (this.listWhere.Count != 0)
				{
					this.objSB.Append(" where");
					foreach (string strWhere in this.listWhere)
					{
						this.objSB.Append(strWhere);
					}
				}
 
				// listGroup
				//		 [GROUP BY Column_List_Item [, ...] ]
				if (this.listGroup.Count != 0)
				{
					objSB.Append(" group by ");
					strComma = "";
					foreach (string strGroup in this.listGroup)
					{
						objSB.Append(strComma).Append(strGroup);
						strComma = ",";
					}
				}
 
				// listHaving
				//		HAVING FilterCondition [AND | OR ...]
				if (this.listHaving.Count != 0)
				{
					objSB.Append(" having");
					foreach (string strHaving in this.listHaving)
					{
						objSB.Append(strHaving);
					}
				}
 
				// listOrder
				//		[ORDER BY Order_Item [ASC | DESC] [, ...]]
				if (this.listOrder.Count != 0)
				{
					if (((this.ServerType == SqlServerType.MsSQL2005) ||
						(this.ServerType == SqlServerType.Oracle) ||
						(this.ServerType == SqlServerType.DB2)) &&
						(this.nCount > 0) &&
						(this.nOffset > 1))
					{
							//符合 MS-SQL 2005, 又啟用分頁功能時, Order子句需放在 rank() 的 over 子句內
							//符合 DB2 或 Oracle, 又啟用分頁功能時, Order子句需放在 dense_rank() 的 over 子句內
					}
					else
					{
						this.AppendOrderString(false);
					}
				}
 
				//開始特製的分頁處理程序
				switch (this.ServerType)
				{
					case SqlServerType.MsSQL:
						if ((this.nCount > 0) && (this.nOffset > 1))
						{
							//先取出之前組合的SQL字串當基礎
							this.strSql_Original = objSB.ToString();
 
							//取得order子句(反向)
							this.objSB.Length = 0;
							if (this.listOrder.Count != 0)
							{
								this.AppendOrderString(true);
							}
							string strSql_Order2 = objSB.ToString();
 
							//取得order子句(正向)
							this.objSB.Length = 0;
							if (this.listOrder.Count != 0)
							{
								this.AppendOrderString(false);
							}
							string strSql_Order3 = objSB.ToString();
 
							//重整分頁模式的SQL字串
							this.objSB.Length = 0;
							this.objSB.AppendFormat(
								"select * from ( select top {0} * from ( {1} ) as tableA {2} ) as tableB {3}",
								this.nCount,
								this.strSql_Original,
								strSql_Order2,
								strSql_Order3);
						}
						break;
					case SqlServerType.MsSQL2005:
						if ((this.nCount > 0) && (this.nOffset > 1))
						{
							//先取出之前組合的SQL字串當基礎
							this.strSql_Original = objSB.ToString();
							this.objSB.Length = 0;
 
							this.objSB.Append("select * from ( select rank() over (");
							this.AppendOrderString(false);					//order子句
							this.objSB.Append(") as RankNumber,* from (");
							this.objSB.Append(this.strSql_Original);		//原本的SQL字串
							this.objSB.Append(") tableA) tableB");
							this.objSB.AppendFormat(" where RankNumber between {0} and {1}", this.nOffset, (this.nOffset + this.nCount - 1));
 
						}
						break;
					case SqlServerType.MySQL:
					case SqlServerType.PostgreSQL:
						if (this.nCount > 0) 
						{
							//LIMIT {[offset,] row_count | row_count OFFSET offset}]
							objSB.AppendFormat(" limit {0} offset {1}", this.nCount, this.nOffset);
						}
						break;
					case SqlServerType.DB2:
					case SqlServerType.Oracle:
						if (this.nCount > 0)
						{
							//先取出之前組合的SQL字串當基礎
							this.strSql_Original = objSB.ToString();
							this.objSB.Length = 0;
 
							this.objSB.Append("select * from ( select dense_rank() over (");
							this.AppendOrderString(false);				//order子句
							this.objSB.Append(") as RankNumber,* from (");
							this.objSB.Append(this.strSql_Original);	//原本的SQL字串
							this.objSB.Append(") tableA) tableB");
							this.objSB.AppendFormat(" where RankNumber between {0} and {1}", this.nOffset, (this.nOffset + this.nCount - 1));
 
						}
						break;
				}
			}
 
			#endregion "SQL字串組合 - select相關"
 
			#region "SQL字串組合 - update與delete相關"
			else if ((this._WorkMode == SqlWorkMode.Update) ||
					(this._WorkMode == SqlWorkMode.Delete))
			{
				//SQL前置字串+where子句
 
				this.objSB.Length = 0;
				this.objSB.Append(this.strSql_Original);
 
				//listWhere
				if (this.listWhere.Count != 0)
				{
					this.objSB.Append(" where");
					foreach (string strWhere in this.listWhere)
					{
						this.objSB.Append(strWhere);
					}
				}
			}
			#endregion "SQL字串組合 - update與delete相關"
 
			return objSB.ToString();
		}
 
 
		public string In(string strFieldName, List<string> listData)
		{
			string strRet = null;
			string strTemp = this.objSB.ToString();
			this.objSB.Length = 0;
			{
				this.objSB.AppendFormat("{0} in (", strFieldName);
				string strComma = "";
				foreach (string strKey in listData)
				{
					this.objSB.Append(strComma).Append(strKey);
					strComma = ",";
				}
				this.objSB.Append(")");
				strRet = objSB.ToString();
			}
			this.objSB.Length = 0;
			this.objSB.Append(strTemp);
 
			return strRet;
		}
 
		public string Like(string strFieldName, string strLikes)
		{
			string strRet = null;
			string strTemp = this.objSB.ToString();
			this.objSB.Length = 0;
			{
			    char[] charSeparators = new char[] {' '};
				string[] result = strLikes.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
				string strComma = "";
				foreach (string strLike in result)
				{
					string strLikeKey = strLike.Trim();
					if (strLikeKey.Length > 0)
					{
						this.objSB.AppendFormat("{0}({1} like '%{2}%')", strComma, strFieldName, strLike.Replace("'", "''"));
						strComma = " and ";
					}
				}
 
				strRet = objSB.ToString();
			}
			this.objSB.Length = 0;
			this.objSB.Append(strTemp);
 
			return strRet;
		}
 
		public SqlStringBuilder insert(string strTable, Dictionary<string, string> setData)
		{
			this._WorkMode = SqlWorkMode.Insert;
			this.objSB.Length = 0;
 
			this.objSB.AppendFormat("insert into {0} (", strTable);
 
			string strComma = "";
			foreach (string strKey in setData.Keys)
			{
				this.objSB.Append(strComma).Append(strKey);
				strComma = ",";
			}
 
			this.objSB.Append(") values (");
 
			strComma = "";
			foreach (string strKey in setData.Keys)
			{
				this.objSB.Append(strComma).Append(setData[strKey]);
				strComma = ",";
			}
			this.objSB.Append(")");
			return this;
		}
 
 
		public SqlStringBuilder update(string strTable, Dictionary<string, string> setData)
		{
			this._WorkMode = SqlWorkMode.Update;
			this.objSB.Length = 0;
 
			this.objSB.AppendFormat("update {0} set ", strTable);
 
			string strComma = "";
			foreach (string strKey in setData.Keys)
			{
				this.objSB.Append(strComma).AppendFormat("{0}={1}", strKey, setData[strKey]);
				strComma = ",";
			}
 
			this.strSql_Original = this.objSB.ToString();
			return this;
		}
 
 
		public SqlStringBuilder delete(string strTable)
		{
			this._WorkMode = SqlWorkMode.Delete;
			this.objSB.Length = 0;
			this.objSB.AppendFormat("delete from {0}", strTable);
			this.strSql_Original = this.objSB.ToString();
			return this;
		}
 
		public SqlStringBuilder select()
		{
			this._WorkMode = SqlWorkMode.Select;
			return this;
		}
 
		public void reset()
		{
			this.strSql_Original = null;
			this.IsDistinct = false;
			this.listQueryFields.Clear();
			this.strIntoTable = null;
			this.listFrom.Clear();
			this.listJoin.Clear();
			this.listWhere.Clear();
			this.listGroup.Clear();
			this.listHaving.Clear();
			this.listOrder.Clear();
			this.nCount = 0;
			this.nOffset = 1;
		}
 
		public SqlStringBuilder distinct()
		{
			this.IsDistinct = true;
			return this;
		}
 
		#region "top子句 -相關函式"
		public SqlStringBuilder top(int count)
		{
			this.nCount = count;
			this.nOffset = 1;
			return this;
		}
 
		public SqlStringBuilder limit(int count)
		{
			this.nCount = count;
			this.nOffset = 1;
			return this;
		}
 
		public SqlStringBuilder limit(int count, int offset)
		{
			this.nCount = count;
			this.nOffset = offset;
			return this;
		}
 
		public SqlStringBuilder page(int pageIndex, int pageSize)
		{
			this.nCount = pageSize;
			this.nOffset = ((pageIndex -1) * pageSize) + 1;
 
			return this;
		}
 
		public SqlStringBuilder all()
		{
			this.nCount = 0;
			this.nOffset = 1;
 
			return this;
		}
		#endregion "top子句 -相關函式"
 
		public SqlStringBuilder into(string strTable)
		{
			//MS-SQL, MS-Access專用SQL語法
			this.strIntoTable = strTable;
			return this;
		}
 
		#region "from子句 - 相關函式"
		public SqlStringBuilder from(string strTable)
		{
			this.listFrom.Add(strTable);
			return this;
		}
 
		public SqlStringBuilder from(string strTable, string strQueryFields)
		{
			this.listFrom.Add(strTable);
			this.listQueryFields.Add(strQueryFields);
			return this;
		}
		#endregion "from子句 - 相關函式"
 
		#region "join子句 - 相關函式"
		public SqlStringBuilder join(JoinType _type, string _strFromTable, bool _IsUsingMode, string _strUsingOrOn)
		{
			this.listJoin.Add(new SqlJoinData(_type, _strFromTable, _IsUsingMode, _strUsingOrOn));
			return this;
		}
 
		public SqlStringBuilder innerJoin_On(string _strFromTable, string _strUsingOrOn)
		{
			return this.join(JoinType.Inner, _strFromTable, false, _strUsingOrOn);
		}
 
		public SqlStringBuilder innerJoin_Using(string _strFromTable, string _strUsingOrOn)
		{
			return this.join(JoinType.Inner, _strFromTable, true, _strUsingOrOn);
		}
 
		public SqlStringBuilder outerJoin_On(string _strFromTable, string _strUsingOrOn)
		{
			return this.join(JoinType.Outer, _strFromTable, false, _strUsingOrOn);
		}
 
		public SqlStringBuilder outerJoin_Using(string _strFromTable, string _strUsingOrOn)
		{
			return this.join(JoinType.Outer, _strFromTable, true, _strUsingOrOn);
		}
 
		public SqlStringBuilder leftJoin_On(string _strFromTable, string _strUsingOrOn)
		{
			return this.join(JoinType.Left, _strFromTable, false, _strUsingOrOn);
		}
 
		public SqlStringBuilder leftJoin_Using(string _strFromTable, string _strUsingOrOn)
		{
			return this.join(JoinType.Left, _strFromTable, true, _strUsingOrOn);
		}
 
		public SqlStringBuilder rightJoin_On(string _strFromTable, string _strUsingOrOn)
		{
			return this.join(JoinType.Right, _strFromTable, false, _strUsingOrOn);
		}
 
		public SqlStringBuilder rightJoin_Using(string _strFromTable, string _strUsingOrOn)
		{
			return this.join(JoinType.Right, _strFromTable, true, _strUsingOrOn);
		}
		#endregion "join子句 - 相關函式"
 
		#region "where子句 - 相關函式"
		public SqlStringBuilder where(string strWhere)
		{
			if (this.listWhere.Count != 0)
			{
				this.listWhere.Add(string.Format(" and ({0})", strWhere));
			}
			else
			{
				this.listWhere.Add(string.Format(" ({0})", strWhere));
			}
			return this;
		}
 
		public SqlStringBuilder orWhere(string strWhere)
		{
			if (this.listWhere.Count != 0)
			{
				this.listWhere.Add(string.Format(" or ({0})", strWhere));
			}
			else
			{
				this.listWhere.Add(string.Format(" ({0})", strWhere));
			}
			return this;
		}
		#endregion "where子句 - 相關函式"
 
		#region "group子句 - 相關函式"
		public SqlStringBuilder group(string strGroup)
		{
			this.listGroup.Add(strGroup);
			return this;
		}
 
		public SqlStringBuilder group(params string[] columns)
		{
			foreach (string strColumn in columns)
			{
				this.listGroup.Add(strColumn);
			}
			return this;
		}
		#endregion "group子句 - 相關函式"
 
		#region "having子句 - 相關函式"
		public SqlStringBuilder having(string strHaving)
		{
			if (this.listHaving.Count != 0)
			{
				this.listHaving.Add(string.Format(" and ({0})", strHaving));
			}
			else
			{
				this.listHaving.Add(string.Format(" ({0})", strHaving));
			}
			return this;
		}
 
		public SqlStringBuilder orHaving(string strHaving)
		{
			if (this.listHaving.Count != 0)
			{
				this.listHaving.Add(string.Format(" or ({0})", strHaving));
			}
			else
			{
				this.listHaving.Add(string.Format(" ({0})", strHaving));
			}
			return this;
		}
		#endregion "having子句 - 相關函式"
 
		#region "order子句 - 相關函式"
		public SqlStringBuilder order(string strField)
		{
			this.listOrder.Add(new SqlOrderData(strField, true));
			return this;
		}
 
		public SqlStringBuilder order(string strField, bool IsASC)
		{
			this.listOrder.Add(new SqlOrderData(strField, IsASC));
			return this;
		}
 
		public SqlStringBuilder order(string strField, string strOrderType)
		{
			this.listOrder.Add(new SqlOrderData(strField, strOrderType));
			return this;
		}
		#endregion "order子句 - 相關函式"
 
	}
}