using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Collections.Generic;
using System.Linq;

namespace  WebApplication1
{
    public class Util
    {
        /// <summary>
        /// 导入第一个sheet的excel数据，转成DataTable
        /// </summary>
        /// <param name="fileUpload"></param>
        /// <returns></returns>
        public static DataTable ExcelToDataTable(HttpPostedFileBase fileUpload)
        {
            DataTable dt = new DataTable();
            int startRow = 0;
            try
            {
                if (fileUpload != null)
                {
                    string extension = Path.GetExtension(fileUpload.FileName);
                    string filename = Path.GetFullPath(fileUpload.FileName);
                    IWorkbook workbook = null;
                    HttpPostedFile file = System.Web.HttpContext.Current.Request.Files[0];
                    MemoryStream mem = new MemoryStream();
                    mem.SetLength((int)file.ContentLength);
                    file.InputStream.Read(mem.GetBuffer(), 0, (int)file.ContentLength);
                    if (extension == ".xlsx")
                    {
                        workbook = new XSSFWorkbook(mem);
                    }
                    else if (extension == ".xls")
                    {
                        workbook = new HSSFWorkbook(mem);
                    }
                    else
                    {
                        throw new Exception("文件格式错误，只允许导入.xls、.xlsx文件");
                    }
                    // 非托管
                    mem.Close();
                    mem.Dispose();
                    ISheet sheet = workbook.GetSheetAt(0);
                    IRow firstRow = sheet.GetRow(0);
                    // 一行最后一个cell的编号 即总的列数
                    int cellCount = firstRow.LastCellNum; 
                    for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                    {
                        ICell cell = firstRow.GetCell(i);
                        if (cell != null)
                        {
                            string cellValue = cell.StringCellValue;
                            if (cellValue != null)
                            {
                                DataColumn column = new DataColumn(cellValue);
                                dt.Columns.Add(column);
                            }
                        }
                    }
                    startRow = sheet.FirstRowNum + 1;
                    //最后一行的标号
                    int rowCount = sheet.LastRowNum;
                    for (int i = startRow; i <= rowCount; ++i)
                    {
                        IRow row = sheet.GetRow(i);
                        //没有数据的行默认是null　　
                        if (row == null) continue;
                        DataRow dataRow = dt.NewRow();
                        for (int j = row.FirstCellNum; j < cellCount; ++j)
                        {
                            if (row.GetCell(j) != null)
                            {
                                dataRow[j] = row.GetCell(j).ToString();
                            }
                        }
                        dt.Rows.Add(dataRow);
                    }
                    return dt;
                }
                else
                {
                    return null;
                }
            }
#pragma warning disable CS0168 // 声明了变量“ex”，但从未使用过
            catch (Exception ex)
#pragma warning restore CS0168 // 声明了变量“ex”，但从未使用过
            {
                // TODO: log
                return null;
            }
        }
        /// <summary>
        /// 根据DataTable生成excel 
        /// </summary>
        public static void RenderToBrowser(List<DataTable> table, List<string> sheetNames, string fileName, HttpResponseBase response, HttpRequestBase request)
        {
            if (request.Browser.Browser == "IE")
                fileName = HttpUtility.UrlEncode(fileName);
            response.AddHeader("Content-Disposition", "attachment;fileName=" + fileName + ".xls");
            response.BinaryWrite(RenderToExcel(table, sheetNames).ToArray());
        }
        public static MemoryStream RenderToExcel(List<DataTable> tables, List<string> sheetNames)
        {
           
            IWorkbook workbook = new HSSFWorkbook();
            int i = 0;
            foreach (var table in tables)
            {
                using (table)
                {
                    ISheet sheet = workbook.CreateSheet(sheetNames[i]);
                    IRow headerRow = sheet.CreateRow(0);
                    // handling header.
                    foreach (DataColumn column in table.Columns)
                        headerRow.CreateCell(column.Ordinal).SetCellValue(column.Caption);//If Caption not set, returns the ColumnName value
                                                                                          // handling value.
                    int rowIndex = 1;
                    foreach (DataRow row in table.Rows)
                    {
                        IRow dataRow = sheet.CreateRow(rowIndex);

                        foreach (DataColumn column in table.Columns)
                        {
                            dataRow.CreateCell(column.Ordinal).SetCellValue(row[column].ToString());
                        }
                        rowIndex++;
                    }
                }
                i++;
            }
            using (MemoryStream ms = new MemoryStream())
            {
                workbook.Write(ms);
                ms.Flush();
                ms.Position = 0;
                return ms;
            }
        }
        /// <summary>
        /// 类似于ExcelToDataTable将包含多个表的Excel转换为DataSet
        /// </summary>
        /// <param name="fileUpload"></param>
        /// <returns></returns>
        public static DataSet ExcelToDataSet(HttpPostedFileBase fileUpload)
        {
            try
            {
                if (fileUpload != null)
                {

                    string extension = Path.GetExtension(fileUpload.FileName);
                    string filename = Path.GetFullPath(fileUpload.FileName);
                    IWorkbook workbook = null;
                    HttpPostedFile file = System.Web.HttpContext.Current.Request.Files[0];
                    MemoryStream mem = new MemoryStream();
                    mem.SetLength((int)file.ContentLength);
                    file.InputStream.Read(mem.GetBuffer(), 0, (int)file.ContentLength);
                    if (extension == ".xlsx")
                    {
                        workbook = new XSSFWorkbook(mem);
                    }
                    else if (extension == ".xls")
                    {
                        workbook = new HSSFWorkbook(mem);
                    }
                    else
                    {
                        throw new Exception("文件格式错误，只允许导入.xls、.xlsx文件");
                    }
                    mem.Close();
                    mem.Dispose();
                    DataSet ds = new DataSet();
                    for (int i = 0; i < workbook.NumberOfSheets; i++)
                    {
                        ISheet sheet = workbook.GetSheetAt(i);
                        DataTable dt = new DataTable(sheet.SheetName);
                        //寻找头列的位置
                        IRow headerRow = sheet.GetRow(sheet.FirstRowNum);
                        for (int j = 0; j < headerRow.LastCellNum; j++)
                        {
                            dt.Columns.Add(headerRow.Cells[j].StringCellValue);
                        }
                        for (int z = (sheet.FirstRowNum + 1); z <=sheet.LastRowNum; z++)
                        {
                            IRow row = sheet.GetRow(z);
                            DataRow dr = dt.NewRow();
                            if (row != null)
                            {
                                //有可能出现溢出错误 改为表头的数量
                                for (int m = row.FirstCellNum; m < dt.Columns.Count; m++)
                                {
                                    if (row.GetCell(m) != null)
                                    {
                                        dr[m] = row.GetCell(m).ToString();
                                    }
                                    else
                                    {
                                        dr[m] = "";
                                    }
                                }
                                dt.Rows.Add(dr);
                            }
                            else
                            {
                                continue;
                            }

                        }
                        ds.Tables.Add(dt);
                    }
                    return ds;

                }
                else
                {
                    return null;
                }
            }
#pragma warning disable CS0168 // 声明了变量“ex”，但从未使用过
            catch (Exception ex)
#pragma warning restore CS0168 // 声明了变量“ex”，但从未使用过
            {

                return null;
            }
        }
        /// <summary>
        /// 身份证合法性检测
        /// </summary>
        /// <param name="IDCard"></param>
        /// <returns></returns>
        public static bool CheckIDCard(string idCard)
        {
            string[] arrVarifyCode = ("1,0,X,9,8,7,6,5,4,3,2").Split(',');
            string[] Wi = ("7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2").Split(',');
            string[] Checker = ("1,9,8,7,6,5,4,3,2,1,1").Split(',');
            string strVerifyCode = string.Empty, Ai = string.Empty, birthday = string.Empty;
            int i = 0, totalmulAiWi = 0, modValue = 0, year = 0, month = 0, day = 0;

            if ((idCard.Length != 18) && (idCard.Length != 15))
            {
                return false;  //身份证位数不对:必须是15位或者18位
            }

            if (idCard.Length == 15)
            {
                Ai = idCard.Substring(0, 6) + "19" + idCard.Substring(6, 9);
            }
            else
            {
                Ai = idCard.Substring(0, 17);
            }
            double tempNum;
            if (!double.TryParse(Ai, out tempNum))
            {
                return false;   //身上证号除最后一位外，其他位不能有非数字字符
            }

            year = Convert.ToInt32(Ai.Substring(6, 4));
            month = Convert.ToInt32(Ai.Substring(10, 2));
            day = Convert.ToInt32(Ai.Substring(12, 2));

            if (month > 12)
            {
                return false;//月份不对，不能大于12  
            }
            if (day > 31)
            {
                return false;  //日期不对，不能大于31
            }

            birthday = year.ToString() + "-" + month.ToString() + "-" + day.ToString();
            DateTime tempDate;
            if (DateTime.TryParse(birthday, out tempDate))
            {
                DateTime DateBirthDay = DateTime.Parse(birthday);
                if (DateBirthDay > DateTime.Now)
                {
                    return false;//年份不对,出生日期不能比当前日期晚
                }
                int intYearLength = DateBirthDay.Year - DateTime.Now.Year;
                if (intYearLength < -140)
                {
                    return false; //身份证输入错误（年份输入错误）！1900+140,即2040年还在用第一代身份证的人这里不认
                }
            }
            else
            {
                return false;   //其中的日期部分转换错误(主要是润月问题)
            }

            //核对校验位
            for (i = 0; i < 17; i++)
            {
                totalmulAiWi = totalmulAiWi + (Convert.ToInt32(Ai.Substring(i, 1)) * Convert.ToInt32(Wi[i].ToString())); //加权
            }
            modValue = totalmulAiWi % 11;  //取模

            strVerifyCode = arrVarifyCode[modValue].ToString(); //校验位
            Ai = Ai + strVerifyCode;
            if (idCard.Length == 18 && idCard != Ai)  //对十八位身份证核对校验位
            {
                return false;//校验位不对
            }
            return true;
        }
        /// <summary>
        /// 从身份证中提取性别和生日
        /// </summary>
        /// <param name="idCard">身份证</param>
        /// <param name="sex">return 性别</param>
        /// <returns>生日</returns>
        public static DateTime GetSexBirthdayByIdCard(string idCard, out string sex)
        {
            string birthday = string.Empty;
            sex = string.Empty;
            if (idCard.Length == 18)
            {
                birthday = idCard.Substring(6, 4) + "-" + idCard.Substring(10, 2) + "-" + idCard.Substring(12, 2);
                sex = idCard.Substring(14, 3);
            }
            if (idCard.Length == 15)
            {
                birthday = "19" + idCard.Substring(6, 2) + "-" + idCard.Substring(8, 2) + "-" + idCard.Substring(10, 2);
                sex = idCard.Substring(12, 3);
            }
            if (int.Parse(sex) % 2 == 0)
            {
                sex = "女";
            }
            else
            {
                sex = "男";
            }
            return DateTime.Parse(birthday);
        }
        /// <summary>
        /// 将不同格式的日期转换为标准时间格式
        /// 传入格式类型限制2016-01-01
        /// 无法进行解析时候返回null
        /// </summary>
        /// <param name="DateTime"></param>
        /// <returns></returns>
        public static DateTime? GetFormatedDate(string dateTimeString)
        {
            try
            {
                if (Regex.IsMatch(dateTimeString, @"^\d{4}-((0[1-9])||(1[0-2]))-\d{2}$"))
                {
                    string[] temp = dateTimeString.Split('-');
                    if (temp.Count() == 1)
                    {
                        int year = Convert.ToInt32(temp[0]);
                        return new DateTime(year, 1, 1);
                    }
                    else if (temp.Count() == 2)
                    {
                        int year = Convert.ToInt32(temp[0]);
                        int month = Convert.ToInt32(temp[1]);
                        return new DateTime(year, month, 1);
                    }
                    else if (temp.Count() == 3)
                    {
                        int year = Convert.ToInt32(temp[0]);
                        int month = Convert.ToInt32(temp[1]);
                        int day = Convert.ToInt32(temp[2]);
                        return new DateTime(year, month, day);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                //解析当前格式错误
                return null;
            }
        }
        /// <summary>
        /// 将不同格式的日期转换为标准期间格式
        /// 传入格式类型限制2016-02
        /// 无法进行解析时候返回null 
        /// </summary>
        /// <param name="dataTimeString"></param>
        /// <returns></returns>
        public static DateTime? GetFormatedPeriod(string dateTimeString)
        {
            try
            {
                if (Regex.IsMatch(dateTimeString, @"^\d{4}-((0[1-9])||(1[0-2]))$"))
                {
                    string[] temp = dateTimeString.Split('-');
                    if (temp.Count() == 1)
                    {
                        int year = Convert.ToInt32(temp[0]);
                        return new DateTime(year, 1, 1);
                    }
                    else if (temp.Count() == 2)
                    {
                        int year = Convert.ToInt32(temp[0]);
                        int month = Convert.ToInt32(temp[1]);
                        return new DateTime(year, month, 1);
                    }
                    else if (temp.Count() == 3)
                    {
                        int year = Convert.ToInt32(temp[0]);
                        int month = Convert.ToInt32(temp[1]);
                        int day = Convert.ToInt32(temp[2]);
                        return new DateTime(year, month, day);
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                //解析当前格式错误
                return null;
            }
        }
        private static bool CheckIDCard18(string idNumber)
        {
            long n = 0;
            if (long.TryParse(idNumber.Remove(17), out n) == false
                || n < Math.Pow(10, 16) || long.TryParse(idNumber.Replace('x', '0').Replace('X', '0'), out n) == false)
            {
                return false;//数字验证  
            }
            string address = "11x22x35x44x53x12x23x36x45x54x13x31x37x46x61x14x32x41x50x62x15x33x42x51x63x21x34x43x52x64x65x71x81x82x91";
            if (address.IndexOf(idNumber.Remove(2)) == -1)
            {
                return false;//省份验证  
            }
            string birth = idNumber.Substring(6, 8).Insert(6, "-").Insert(4, "-");
            DateTime time = new DateTime();
            if (DateTime.TryParse(birth, out time) == false)
            {
                return false;//生日验证  
            }
            string[] arrVarifyCode = ("1,0,x,9,8,7,6,5,4,3,2").Split(',');
            string[] Wi = ("7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2").Split(',');
            char[] Ai = idNumber.Remove(17).ToCharArray();
            int sum = 0;
            for (int i = 0; i < 17; i++)
            {
                sum += int.Parse(Wi[i]) * int.Parse(Ai[i].ToString());
            }
            int y = -1;
            Math.DivRem(sum, 11, out y);
            if (arrVarifyCode[y] != idNumber.Substring(17, 1).ToLower())
            {
                return false;//校验码验证  
            }
            return true;//符合GB11643-1999标准  
        }
        /// <summary>
        /// 校验身份证信息
        /// 普通正则校验后再函数校验提高校验速度
        /// </summary>
        /// <param name="idNumber"></param>
        /// <returns></returns>
        public static bool CheckIDCardNew(string idNumber)
        {
            return Regex.IsMatch(idNumber, @"^(\d{15}$|^\d{18}$|^\d{17}(\d|X|x))$", RegexOptions.IgnoreCase);
        }
        /// <summary>
        /// 校验邮箱信息
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool CheckEmail(string email)
        {
            Regex r = new Regex("^\\s*([A-Za-z0-9_-]+(\\.\\w+)*@(\\w+\\.)+\\w{2,5})\\s*$");
            return r.IsMatch(email);
        }
        public static bool CheckPhoneNum(string phoneNum)
        {
            Regex re = new Regex(@"^1([358][0-9]|4[579]|66|7[0135678]|9[89])[0-9]{8}$");
            return re.IsMatch(phoneNum);
        }
        /// <summary>
        /// AddressString一般来说为XXX省XXX市的格式 不严格判定
        /// 1、会先去判定是否为直辖市
        /// 2、不是直辖市取出里面的地点信息，过滤掉其他的信息
        /// </summary>
        /// <param name="AddressString"></param>
        /// <returns></returns>
        public static Address GetAddress(string AddressString)
        {
            try
            {
                string[] selfGovCity = { "北京", "上海", "天津", "重庆" };
                foreach (var s in selfGovCity)
                {
                    if (AddressString.Contains(s))
                    {
                        return new Address() { IsSelfGov = true, Province = s };
                    }
                }
                //暂时不判断省份和市名是否准确
                int indexProvince = AddressString.IndexOf("省");
                int indexCity = AddressString.IndexOf("市");
                string province = AddressString.Substring(0, indexProvince+1);
                string city = AddressString.Substring(indexProvince + 1, indexCity - indexProvince);
                return new Address() { IsSelfGov = false, Province = province, City = city };
            }
            catch (Exception)
            {

                return null;
            }
        }
        public class Address
        {
            public Address()
            { }
            public bool IsSelfGov = false;
            public string Province { get; set; }
            public string City { get; set; }
            //public string County { get; set; }
        }
        /// <summary>
        /// 判断当前银行是否在列表里面
        /// </summary>
        /// <returns></returns>
        public static bool CheckBankName(string bankName)
        {
            try
            {
                string[] bankNameList = {"中国银行","中国农业银行","中国建设银行","中国工商银行","交通银行","招商银行","中国民生银行","中国光大银行","中信银行","中国邮政储蓄银行","华夏银行","宁波银行","成都农村商业银行","深圳农村商业银行","平安银行","成都银行","长城华西银行","廊坊银行","广汉珠江村镇银行","新津珠江村镇银行","贵阳银行"};
                return Array.IndexOf(bankNameList, bankName) != -1;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 获取员工导入的错误提示
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static string GetErrorTips(string tableName, string errorType, string errorName)
        {
            return string.Format("{0}中出现错误 \r\n\r\n错误类型：{1} \r\n员工名称：{2}",tableName, errorType, errorName);
        }
        /// <summary>
        /// 检查对象是否为int类型
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsInt(string value)
        {
            return Regex.IsMatch(value, @"^[+-]?\d*$");
        }
        /// <summary>
        /// 检查对象是否为double类型
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsDouble(string value)
        {
            return Regex.IsMatch(value, @"^-?\d+\.?\d+$");
        }
        /// <summary>
        /// 将object类型转换为可空的int类型
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static int? ConvertObjectToNullableInt(object o)
        {
            int? result = null;
            if (o.ToString().Trim() == "")
            {
                result = -1;
            }
            else
            {
                if (IsInt(o.ToString().Trim()))
                {
                    result = Convert.ToInt32(o.ToString().Trim());
                }
            }
            return result;
        }
        /// <summary>
        /// 将object类型转换为可空的double类型
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static double? ConvertObjectToNullableDouble(object o)
        {
            double? result = null;
            if (o.ToString().Trim() == "")
            {
                result = -1;
            }
            else
            {
                if (IsDouble(o.ToString().Trim()))
                {
                    result = Convert.ToDouble(o.ToString().Trim());
                }
            }
            return result;
        }
        /// <summary>
        /// 分页工具类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class PagedList<T> : List<T>
        {
            #region Properties

            public int PageIndex { get; private set; }

            public int PageSize { get; private set; }

            public int TotalCount { get; private set; }

            public int TotalPages { get; private set; }

            public bool HasPreviousPage
            {
                get { return (PageIndex > 0); }
            }
            public bool HasNextPage
            {
                get { return (PageIndex + 1 < TotalPages); }
            }

            #endregion
            #region Constructors

            public PagedList(IQueryable<T> source, int pageIndex, int pageSize)
            {
                if (source == null || source.Count() < 1)
                    throw new System.ArgumentNullException("source");

                int total = source.Count();
                this.TotalCount = total;
                this.TotalPages = total / pageSize;

                if (total % pageSize > 0)
                    TotalPages++;

                this.PageSize = pageSize;
                this.PageIndex = pageIndex;
                this.AddRange(source.Skip((pageIndex-1) * pageSize).Take(pageSize).ToList());
            }

            public PagedList(IList<T> source, int pageIndex, int pageSize)
            {
                if (source == null || source.Count() < 1)
                    throw new System.ArgumentNullException("source");

                TotalCount = source.Count();
                TotalPages = TotalCount / pageSize;

                if (TotalCount % pageSize > 0)
                    TotalPages++;

                this.PageSize = pageSize;
                this.PageIndex = pageIndex;
                this.AddRange(source.Skip((pageIndex-1) * pageSize).Take(pageSize).ToList());
            }

            //public PagedList(IEnumerable<T> source, int pageIndex, int pageSize, int totalCount)
            //{
            //    if (source == null || source.Count() < 1)
            //        throw new System.ArgumentNullException("source");

            //    TotalCount = totalCount;
            //    TotalPages = TotalCount / pageSize;

            //    if (TotalCount % pageSize > 0)
            //        TotalPages++;

            //    this.PageSize = pageSize;
            //    this.PageIndex = pageIndex;
            //    this.AddRange(source);
            //}

            #endregion
        }
    }
}
