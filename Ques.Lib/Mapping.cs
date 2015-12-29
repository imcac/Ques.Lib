using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.UI;

namespace Ques.Lib
{
    public class Mapping
    {
        static Assembly thmjLib = Assembly.Load(ConfigurationManager.AppSettings["ModelMapping"]);
        public static void BindFormData(object main, object obj, Type type)
        {
            
            if (obj == null || main == null)
            {
                throw new Exception("参数不可为Null");
            }
            object refObj = thmjLib.CreateInstance(type.ToString());
            PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo p in props)
            {
                Control refCtrl = (main as System.Web.UI.Page).FindControl(p.Name);
                object value = null;
                if (refCtrl != null)
                {
                    switch (p.PropertyType.BaseType.ToString())
                    {
                        case "System.Enum":
                            var enumObj = thmjLib.CreateInstance(p.PropertyType.ToString());
                            value = p.GetValue(obj, new string[] { });
                            value = (int)Enum.Parse(thmjLib.CreateInstance(p.PropertyType.ToString()).GetType(), value + string.Empty);
                            break;
                        default:
                            value = p.GetValue(obj, new string[] { });
                            break;
                    }
                    if (value != null)
                    {
                        SetValue2Ctrl(refCtrl, value.ToString());
                    }
                }
            }
        }
        public static object ConvertJson2Model(Type type, object model, JsonParamsHelper jsonPH)
        {
            PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo p in props)
            {
                string value = jsonPH.GetParamByKey(p.Name);
                if (!string.IsNullOrEmpty(value))
                {
                    switch (p.PropertyType.BaseType.ToString())
                    {
                        case "System.Enum":

                            var enumObj = thmjLib.CreateInstance(p.PropertyType.ToString());
                            if (enumObj != null)
                            {
                                p.SetValue(model, Enum.Parse(thmjLib.CreateInstance(p.PropertyType.ToString()).GetType(), value + string.Empty), new string[] { });
                            }
                            break;
                        case "System.ValueType":
                            switch (p.PropertyType.FullName)
                            {
                                case "System.Int32":
                                    p.SetValue(model, value, new string[] { });
                                    break;
                                case "System.Guid":
                                    p.SetValue(model, Guid.Parse(value), new string[] { });
                                    break;
                                case "System.String":
                                    p.SetValue(model, value, new string[] { });
                                    break;
                                case "System.DateTime":
                                    p.SetValue(model, DateTime.Parse(value), new string[] { });
                                    break;
                                default:
                                    //p.SetValue(model, value, new string[] { });
                                    break;
                            }
                            break;
                        default:
                            p.SetValue(model, value, new string[] { });
                            break;
                    }
                }
            }
            return model;
        }
        public static object ConvertJson2Model(Type type, JsonParamsHelper jsonPH)
        {
            return ConvertJson2Model(type, Activator.CreateInstance(type), jsonPH);
        }
        public static void SetValue2Ctrl(Control ctrl, string value)
        {
            switch (ctrl.GetType().ToString())
            {
                case "System.Web.UI.HtmlControls.HtmlInputHidden":
                    (ctrl as System.Web.UI.HtmlControls.HtmlInputHidden).Value = value;
                    break;
                case "System.Web.UI.HtmlControls.HtmlInputText":
                    (ctrl as System.Web.UI.HtmlControls.HtmlInputText).Value = value;
                    break;
                case "System.Web.UI.WebControls.DropDownList":
                    (ctrl as System.Web.UI.WebControls.DropDownList).SelectedValue = value;
                    break;
                case "System.Web.UI.HtmlControls.HtmlSelect":
                    (ctrl as System.Web.UI.HtmlControls.HtmlSelect).Value = value;
                    break;
                    //case "Controls_RichContent":
                    //    break;
            }
        }
    }
    public class JsonParamsHelper
    {
        private JObject jsonObj;
        public JsonParamsHelper(string postParams)
        {
            jsonObj = (JObject)JsonConvert.DeserializeObject(JsonConvert.DeserializeObject(postParams).ToString());
        }
        public string GetParamByKey(string key)
        {
            if (jsonObj == null)
            {
                throw new Exception("没有被初始化");
            }
            try
            {

                return jsonObj.GetValue(key, StringComparison.InvariantCultureIgnoreCase).Value<string>();
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
