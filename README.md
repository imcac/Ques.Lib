# Ques.Lib

通过反射对实体类进行赋值以及循环绑定操作，目前主要作用于Asp.net+WebService

常规用法 主要内容是Mapping类中的两个静态方法。

form表单（表单Name以及Id需要对应实体类的属性，如需使用该工具进行绑定，请给input加入runat="server"）：

<form class="form-horizontal m_t_20p" role="form">
    <div class="form-group">
        <label for="firstname" class="col-sm-2 control-label">用户名</label>
        <div class="col-sm-10">
            <input type="text" class="form-control" id="LoginName" name="LoginName"
                placeholder="请输入用户名">
        </div>
    </div>
    <div class="form-group">
        <label for="lastname" class="col-sm-2 control-label">密码</label>
        <div class="col-sm-10">
            <input type="password" class="form-control" id="Password" name="Password"
                placeholder="请输密码">
        </div>
    </div>
    <div class="form-group">
        <div class="col-sm-offset-2 col-sm-10">
            <div class="checkbox">
                <label>
                    <input type="checkbox" />
                    请记住我
                </label>
            </div>
        </div>
    </div>
    <div class="form-group">
        <div class="col-sm-offset-2 col-sm-10">
            <a id="denglu" href="#" class="btn bg_c_b c_w b_r_40p" role="button">登录
            </a>
            <a href="/Regist" class="btn bg_c_gr c_w b_r_40p" role="button">注册
            </a>
        </div>
    </div>
</form>
前端代码（serializeString需要特定方法，LZ会上传更新该js函数）：
    
       $.ajax({
           type: "post",
           dataType: "json",
           contentType: "application/json", //注意：WebMethod()必须加这项，否则客户端数据不会传到服务端
           data: JSON.stringify($("form").serializeString()),//注意：data参数可以是string个int类型
           url: "/Views/Account/Login.aspx/Login",//模拟web服务，提交到方法
           // 可选的 async:false,阻塞的异步就是同步
           beforeSend: function () {
               // do something.
               // 一般是禁用按钮等防止用户重复提交
               $("#denglu").attr({ disabled: "disabled" });
               // 或者是显示loading图片
           },
           success: function (data) {
               $("#denglu").removeAttr("disabled");
               console.log(data);
               var rurl = '<%= Request["rurl"] %>';
               if (rurl) {
                   window.location.href = rurl;
               }
               else {
                   window.location.href = "/";
               }
               // 服务端可以直接返回Model，也可以返回序列化之后的字符串，如果需要反序列化：string json = JSON.parse(data.d);
               // 有时候需要嵌套调用ajax请求，也是可以的
           },
           complete: function () {
               //do something.
               $("#btnClick").removeAttr("disabled");
               // 隐藏loading图片
           },
           error: function (data) {
               $("#denglu").removeAttr("disabled");
               console.log(data);
               alert("error: " + data.d);
           }
       });
后台调用对应的方法：

[WebMethod(EnableSession = true)]
public static string Login(string postParams)
{
    JsonParamsHelper jsonPH = new JsonParamsHelper(HttpContext.Current.Server.UrlDecode(postParams));
    try
    {
        AccountInfoBLL accountBLL = new AccountInfoBLL();
        AccountInfo model = accountBLL.GetByLoginName(jsonPH.GetParamByKey("LoginName"));
        if (model.Password.Equals(jsonPH.GetParamByKey("Password")))
        {
            SetCurrentLoginUser(model);
            return ResponseCode.WriteResult(ResCode.成功, "登录成功", CreateId().ToString(), postParams
                , typeof(Views_Account_Login), System.Reflection.MethodBase.GetCurrentMethod().Name);
        }
        else if (model == null)
        {
            return ResponseCode.WriteResult(ResCode.失败, "用户不存在", CreateId().ToString(), postParams
                , typeof(Views_Account_Login), System.Reflection.MethodBase.GetCurrentMethod().Name);
        }
        else
        {
            return ResponseCode.WriteResult(ResCode.失败, "密码错误", CreateId().ToString(), postParams
                , typeof(Views_Account_Login), System.Reflection.MethodBase.GetCurrentMethod().Name);
        }
    }
    catch (Exception ex)
    {
        return ResponseCode.WriteResult(ResCode.错误, "Submit 错误！" + ex.Message, CreateId().ToString(), ex.Message
            , typeof(Views_Account_Login), System.Reflection.MethodBase.GetCurrentMethod().Name);
    }
}
