using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Collections.Generic;
 
namespace com.data
{
/// <summary>
/// SqlParameterHelper 的摘要描述
/// </summary>
public class SqlParameterHelper
{
	public SqlParameterHelper()
	{
		//
		// TODO: 在此加入建構函式的程式碼
		//
	}
 
	//public ArrayList Parameters = new ArrayList();
	public List<SqlParameter> Parameters = new List<SqlParameter>();
 
	//字串分割的元素
	const string strSeparatorLeft	= "[";
	const string strSeparatorRight  = "]=";
	string [] strSeparatorColumn	= new string [] { "," };
	string [] strSeparatorRow	= new string [] {";"};
 
	//"%" = "%25"
	//";" = "%3B"
	//"<" = "%3C"		不用"&lt;"
	//">" = "%3E"		不用"&gt;"
	//"=" = "%3D"
	//"," = "%2C"
	//"[" = "%5B"
	//"]" = "%5D"
 
 
 
	string ToFixString(string strString)
	{
		//XML保留字元轉換
		string strFixString = strString;
 
		strFixString = strFixString.Replace("%", "%25");
		strFixString = strFixString.Replace(";", "%3B");
 
		strFixString = strFixString.Replace("<", "%3C");
		strFixString = strFixString.Replace(">", "%3E");
 
		strFixString = strFixString.Replace("=", "%3D");
		strFixString = strFixString.Replace(",", "%2C");
 
		strFixString = strFixString.Replace("[", "%5B");
		strFixString = strFixString.Replace("]", "%5D");
 
		return strFixString;
 
	}
 
	string FromFixString(string strFixString)
	{
		string strString = strFixString;
 
		strString = strString.Replace("%5D", "]");
		strString = strString.Replace("%5B", "[");
 
		strString = strString.Replace("%2C", ",");
		strString = strString.Replace("%3D", "=");
 
		strString = strString.Replace("%3E", ">");
		strString = strString.Replace("%3C", "<");
 
		strString = strString.Replace("%3B", ";");
		strString = strString.Replace("%25", "%");
 
		return strString;
	}
 
	public void FromSerializeString(string strParameter)
	{
		//Parameters.Clear();
 
		string[] strDataRows = strParameter.Split(strSeparatorRow, StringSplitOptions.None);
		foreach (string strRow in strDataRows)
		{
			string		strName		= "";
 
			string		strType		= "NVarChar";
			SqlDbType	objType		= SqlDbType.NVarChar;
 
			int			nSize		= 0;
			byte		nScale		= 0;
			bool		IsNull = false;
 
			//(FixIt)
			string strValue = FromFixString(strRow.Substring(strRow.IndexOf(strSeparatorRight) + strSeparatorRight.Length));
 
			object		objValue	= null;
 
			string strAttribs = strRow.Substring(	strRow.IndexOf(strSeparatorLeft) + strSeparatorLeft.Length ,
													strRow.IndexOf(strSeparatorRight) - strSeparatorLeft.Length );
 
			string [] strDataLine = strAttribs.Split(strSeparatorColumn, StringSplitOptions.None);
			int i = 0;
			foreach (string strColumnValue in strDataLine)
			{
				switch (i)
				{
					case 0:
						//(FixIt)
						strName = FromFixString(strColumnValue);
						break;
					case 1:
						strType = strColumnValue;
						break;
					default:
						if ((strColumnValue != null) &&
							(strColumnValue != ""))
						{
							int nFind = strColumnValue.IndexOf("=");
							if (nFind != -1)
							{
								string strAttribName =  strColumnValue.Substring(0, nFind-1);
								string strAttribValue = strColumnValue.Substring(nFind + 1);
 
								switch (strType)
								{
									case "Size":
										try
										{
											nSize = int.Parse(strAttribValue);
										}
										catch
										{
											nSize = 0;
										}
										break;
									case "Scale":
										try
										{
											nScale = byte.Parse(strAttribValue);
										}
										catch
										{
											nScale = 0;
										}
										break;
									case "Null":
										IsNull = true;
										break;
									default:
										break;
								}
							}
						}
						break;
				}
				i++;
			}
 
			switch (strType)
			{
			//SqlDbType.BigInt;
				case "BigInt":
					objType = SqlDbType.BigInt;
					try
					{
						objValue = Int64.Parse(strValue);
					}
					catch
					{
						objValue = (Int64) 0;
					}
					break;
 
			//SqlDbType.Binary;
 
 
			//SqlDbType.Bit;
				case "Bit":
					objType = SqlDbType.Bit;
					try
					{
						objValue = Boolean.Parse(strValue);
					}
					catch
					{
						objValue = (Boolean) false;
					}
					break;
 
			//SqlDbType.Char;
 
 
			//SqlDbType.DateTime;
				case "DateTime":
					objType = SqlDbType.DateTime;
					try
					{
						objValue = DateTime.Parse(strValue);
					}
					catch
					{
						objValue = DateTime.MinValue;
					}
					break;
 
			//SqlDbType.Decimal;
				case "Decimal":
					objType = SqlDbType.Decimal;
					try
					{
						objValue = Decimal.Parse(strValue);
					}
					catch
					{
						objValue = (Decimal) 0;
					}
					break;
 
			//SqlDbType.Float;
				case "Float":
					objType = SqlDbType.Float;
					try
					{
						objValue = Double.Parse(strValue);
					}
					catch
					{
						objValue = (Double) 0.0f;
					}
					break;
 
			//SqlDbType.Image;
 
 
			//SqlDbType.Int;
				case "Int":
					objType	 = SqlDbType.Int;
					try 
					{
						objValue = Int32.Parse(strValue);
					}
					catch
					{
						objValue = (Int32) 0; 
					}
					break;
 
			//SqlDbType.Money;
				case "Money":
					objType = SqlDbType.Money;
					try
					{
						objValue = Decimal.Parse(strValue);
					}
					catch
					{
						objValue = (Decimal) 0;
					}
					break;
 
			//SqlDbType.NChar;
				case "NChar":
					objType = SqlDbType.NChar;
					try
					{
						objValue = strValue;
					}
					catch
					{
						objValue = (string) "";
					}
					break;
 
			//SqlDbType.NText;
				case "NText":
					objType = SqlDbType.NText;
					try
					{
						objValue = strValue;
					}
					catch
					{
						objValue = (string) "";
					}
					break;
 
			//SqlDbType.NVarChar;
				case "NVarChar":
					objType = SqlDbType.NVarChar;
					try
					{
						objValue = strValue;
					}
					catch
					{
						objValue = (string) "";
					}
					break;
 
			//SqlDbType.Real;
				case "Real":
					objType = SqlDbType.Real;
					try
					{
						objValue = Single.Parse(strValue);
					}
					catch
					{
						objValue = (Single) 0.0f;
					}
					break;
 
			//SqlDbType.SmallDateTime;
				case "SmallDateTime":
					objType = SqlDbType.SmallDateTime;
					try
					{
						objValue = DateTime.Parse(strValue);
					}
					catch
					{
						objValue = DateTime.MinValue;
					}
					break;
 
			//SqlDbType.SmallInt;
				case "SmallInt":
					objType = SqlDbType.SmallInt;
					try
					{
						objValue = Int16.Parse(strValue);
					}
					catch
					{
						objValue = (Int16) 0;
					}
					break;
 
			//SqlDbType.SmallMoney;
				case "SmallMoney":
					objType = SqlDbType.SmallMoney;
					try
					{
						objValue = Decimal.Parse(strValue);
					}
					catch
					{
						objValue = (Decimal) 0;
					}
					break;
 
			//SqlDbType.Text;
			//SqlDbType.Timestamp;
 
			//SqlDbType.TinyInt;
				case "TinyInt":
					objType = SqlDbType.TinyInt;
					try
					{
						objValue = Byte.Parse(strValue);
					}
					catch
					{
						objValue = (Byte) 0;
					}
					break;
 
			//SqlDbType.Udt;
			//SqlDbType.UniqueIdentifier;
				case "UniqueIdentifier":
					objType = SqlDbType.UniqueIdentifier;
					try
					{
						objValue = new Guid(strValue);
					}
					catch
					{
						objValue = Guid.Empty;
					}
					break;
 
			//SqlDbType.VarBinary;
			//SqlDbType.VarChar;
			//SqlDbType.Variant;
			//SqlDbType.Xml;
 
				default:				  
					objType	 = SqlDbType.NVarChar;
					objValue = strValue;
					break;
			}
 
			SqlParameter objParameter;
 
			if (nSize == 0)
			{
				objParameter = new SqlParameter(strName, objType);
			}
			else
			{
				objParameter = new SqlParameter(strName, objType, nSize);
			}
			if (nScale != 0)
			{
				objParameter.Scale = nScale;
			}
 
			if (IsNull)
			{
				objParameter.IsNullable = true;
				objParameter.Value = null;
			}
			else
				objParameter.Value = objValue;
 
			this.Parameters.Add(objParameter);
		}
	}
 
	public string ToSerializeString()
	{
		StringBuilder objSB = new StringBuilder();
 
		//資料列分隔符號
		string strSeparator = "";	
 
		foreach (SqlParameter objParameter in Parameters)
		{
			objSB.Append(strSeparator);
			objSB.Append(strSeparatorLeft);
			objSB.Append(ToFixString(objParameter.ParameterName));				//參數名稱(FixIt)
 
			if (objParameter.SqlDbType != SqlDbType.NVarChar)
			{
				objSB.Append(strSeparatorColumn[0]);
				objSB.Append(objParameter.SqlDbType.ToString());				//參數DataType
 
  				if (objParameter.Size != 0)										//參數的Size
				{
					objSB.Append(",Size=").Append(objParameter.Size);
				}
			}
 
			if (objParameter.Scale != 0)										//參數的Scale
			{
				objSB.Append(",Scale=").Append(objParameter.Scale);
			}
 
			if (objParameter.Value == null) 
			{
				objSB.Append(",Null");
			}
 
 
 
			objSB.Append(strSeparatorRight);
			if (objParameter.Value != null) 
				objSB.Append(ToFixString(objParameter.Value.ToString()));			//參數Data(FixIt)
			strSeparator = strSeparatorRow[0];
		}
 
		return objSB.ToString();
	}
}
}