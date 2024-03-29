/*
 * Translated default messages for the jQuery validation plugin.
 * Locale: CN
 */
jQuery.extend(jQuery.validator.messages, {
        required: "该字段不能为空",
		remote: "请修正该字段",
		email: "请输入正确格式的电子邮件",
		url: "请输入合法的网址",
		date: "请输入合法的日期",
		dateISO: "请输入合法的日期 (ISO).",
		number: "请输入合法的数字",
		digits: "只能输入整数",
		creditcard: "请输入合法的信用卡号",
		equalTo: "请再次输入相同的值",
		accept: "请输入拥有合法后缀名的字符串",
		maxlength: jQuery.validator.format("请输入一个长度最多是 {0} 的字符串"),
		minlength: jQuery.validator.format("请输入一个长度最少是 {0} 的字符串"),
		rangelength: jQuery.validator.format("请输入一个长度介于 {0} 和 {1} 之间的字符串"),
		range: jQuery.validator.format("请输入一个介于 {0} 和 {1} 之间的值"),
		max: jQuery.validator.format("请输入一个最大为 {0} 的值"),
		min: jQuery.validator.format("请输入一个最小为 {0} 的值"),

        notnull:"不能为空"
});
/*****************************************************************
                  jQuery Validate扩展验证方法  (linjq)       
*****************************************************************/
$(function () {
    // 电话号码验证    
    jQuery.validator.addMethod("isTelTow", function (value, element) {
        if (value !== "") {
            var tel = /([0]{1}[1-9]{2,3}-)?[0-9]{7,8}/;    //电话号码格式010-12345678
            return this.optional(element) || (tel.test(value));
        } else {
            return true;
        }
    }, "请正确填写您的电话号码。");

    // 联系电话(手机/固定电话皆可)验证   
    jQuery.validator.addMethod("isTel", function (value, element) {
        if (value !== "") {
            var length = value.length;
            var mobile = /^(((13[0-9]{1})|(15[0-9]{1})|(17[0-9]{1})|(18[0-9]{1}))+\d{8})$/;
            var tel = /^(\d{3,4}-?)?\d{7,9}$/g;
            return this.optional(element) || tel.test(value) || (length == 11 && mobile.test(value));
        } else {
            return true;
        }
    }, "请正确填写您的联系方式");

    // 匹配密码，以字母开头，长度在6-12之间，只能包含字符、数字和下划线。      
    jQuery.validator.addMethod("isPwd", function (value, element) {
        if (value !== "") {
            return this.optional(element) || /^[a-zA-Z]\\w{6,20}$/.test(value);
        } else {
            return true;
        }
    }, "以字母开头，长度在6-20之间，只能包含字符、数字和下划线。");

    // 字符验证，只能包含中文、英文、数字、下划线等字符。    
    jQuery.validator.addMethod("stringCheck", function (value, element) {
        if (value !== "") {
            return this.optional(element) || /^[a-zA-Z0-9\u4e00-\u9fa5-_+]+$/.test(value);
        } else {
            return true;
        }
    }, "只能包含中文、英文、数字、下划线等字符");

    // 匹配汉字 
    jQuery.validator.addMethod("isChinese", function (value, element) {
        if (value !== "") {
            return this.optional(element) || /^[\u4e00-\u9fa5]+$/.test(value);
        } else {
            return true;
        }
    }, "匹配汉字");

    // 手机号码验证    
    jQuery.validator.addMethod("isMobile", function (value, element) {
        if (value !== "") {
            var length = value.length;
            return this.optional(element) || (length == 11 && /^(((13[0-9]{1})|(15[0-9]{1})|(17[0-9]{1})|(18[0-9]{1}))+\d{8})$/.test(value));
        } else {
            return true;
        }
    }, "请正确填写您的手机号码。");

    // 电话号码验证    
    jQuery.validator.addMethod("isTel", function (value, element) {
        if (value !== "") {
            return /^(0[0-9]{2,3}\-)?([2-9][0-9]{6,7})+(\-[0-9]{1,4})$/.test(value);
        } else {
            return true;
        }
    }, "请正确填写您的电话号码。");

    jQuery.validator.addMethod("ContentType", function (value, element) {
        if (value !== "") {
            return this.optional(element) || (/^(0[0-9]{2,3}\-)?([2-9][0-9]{6,7})+(\-[0-9]{1,4})?$|^((13[0-9]|15[0-9]|17[0-9]|18[0-9])\d{8}$)/.test(value));
        } else {
            return true;
        }
    }, "请正确填写您的联系电话。");

    // 只能输入[0-9]数字
    jQuery.validator.addMethod("isDigits", function (value, element) {
        if (value !== "") {
            return this.optional(element) || /^\d+$/.test(value);
        } else {
            return true;
        }
    }, "只能输入0-9数字");

    // 只能输入1-9正整数
    jQuery.validator.addMethod("posint", function (value, element) {
        if (value !== "") {
            return this.optional(element) || /^[1-9]\d*$/.test(value);
        } else {
            return true;
        }
    }, "只能输入1-9正整数");

    // 保留至多两位小数
    jQuery.validator.addMethod("posintdec", function (value, element) {
        if (value !== "") {
            return this.optional(element) || /^(?!0+(?:\.0+)?$)(?:[1-9]\d*|0)(?:\.\d{1,2})?$/.test($.trim(value));
        } else {
            return true;
        }
    }, "保留至多两位小数");

    // 身份证验证
    jQuery.validator.addMethod("cardid", function (value, element) {
        if (value !== "") {
            return this.optional(element) || /(^\d{15}$)|(^\d{18}$)|(^\d{17}(\d|X|x)$)/.test($.trim(value));
        } else {
            return true;
        }
    }, "请输入正确身份证");

    // 保留至多5位小数
    jQuery.validator.addMethod("posintdec3", function (value, element) {
        if (value !== "") {
            return this.optional(element) || /^[0-9]+\.{0,1}[0-9]{0,5}$/.test(value);
        } else {
            return true;
        }
    }, "保留至多两位小数");

    //总楼层大于或等于所在楼层
    jQuery.validator.addMethod("larger", function (value, element) {
        var sumreward = $("#Floor").val();
        var flag = false;
        if (sumreward == null || sumreward == "" || value == null || value == "") {
            flag = true;
        } else {
            if (parseInt(value) >= parseInt(sumreward)) {
                flag = true;
            } else {
                flag = false;
            }
        }
        return flag;
    }, $.validator.format("总楼层不能低于所在楼层!"));


    // 最大长度（字符2个 字母数字1个）    
    jQuery.validator.addMethod("byteRangeLength", function (value, element, param) {
        var length = value.length;
        for (var i = 0; i < value.length; i++) {
            if (value.charCodeAt(i) > 127) {
                length++;
            }
        }
        return this.optional(element) || (length <= param);
    }, $.validator.format("长度不能超过{0}个字符"));

    //整数
    jQuery.validator.addMethod("Integer", function (value, element) {
        return this.optional(element) || (/^-?[1-9]\d*$/.test(value));
    }, "请输入整数");


    jQuery.validator.addMethod("isUseStateLeasePeriod", function (value, element, param) {
        var flag = false;
        var _value = $.trim($("#UseState").val());
        switch (_value) {
            case '出租':
                if ($.trim(value) !== "") {
                    flag = _flag(value, flag, param);
                } else {
                    flag = false;
                }
                break;
            case '闲置':
                if ($.trim(value) !== "") {
                    flag = _flag(value, flag, param);
                } else {
                    flag = false;
                }
                break;
            case '':
                flag = true;
                break;
            default:
                flag = true;
                break;
        }
        return flag;
    }, $.validator.format("必填项且只允许输入数字，小数点前最多保留{0}位数字，小数点后最多能保留2位小数"));

    jQuery.validator.addMethod("isUseStatePeriodicUnit", function (value, element) {
        var flag = false;
        var _value = $.trim($("#UseState").val());
        switch (_value) {
            case '出租':
                if ($.trim(value) !== "") {
                    flag = true;
                } else {
                    flag = false;
                }
                break;
            case '':
                flag = true;
                break;
            default:
                flag = true;
                break;
        }
        return flag;
    }, "请输入必填项");

    jQuery.validator.addMethod("isUseStateIdlePeriod", function (value, element) {
        var flag = false;
        var _value = $.trim($("#UseState").val());
        switch (_value) {
            case '闲置':
                if ($.trim(value) !== "") {
                    if (/^-?[1-9]\d*$/.test(value)) {
                        flag = true;
                    } else {
                        flag = false;
                    }
                } else {
                    flag = false;
                }
                break;
            case '':
                flag = true;
                break;
            default:
                flag = true;
                break;
        }
        return flag;
    }, "请输入必填项");

    //只允许输入数字，小数点前最多保留14位数字，小数点后最多能保留2位小数
    jQuery.validator.addMethod("specialNumber", function (value, element, param) {
        var flag = false;
        flag = _flag(value, flag, param);
        return flag;
    }, $.validator.format("请输入数字小数点前最多保留{0}位数字，小数点后最多能保留2位小数"));

    // 建成年代 1900~2049
    jQuery.validator.addMethod("buildYear", function (value, element) {
        var flag = false;
        if (("" + value).length != 4) {
            flag = false;
        } else {
            if (Number(value) >= 1900 && Number(value) <= 2049) {
                flag = true;
            } else {
                flag = false;
            }
        }
        return flag;
    }, "请输入1900~2049的整数");

    // 验证码四位数
    jQuery.validator.addMethod("validateCode", function (value, element) {
        return /^[0-9A-Za-z]{4,4}$/.test(value);
    }, "请输入正确的验证码");

    //时间格式验证
    jQuery.validator.addMethod("DateFormat", function (value, element) {
        return this.optional(element) || (/^(\d{4})-(0\d{1}|1[0-2])-(0\d{1}|[12]\d{1}|3[01])$/.test(value));
    }, "输入日期格式(yyyy-mm-dd)");

    // 空白字符
    jQuery.validator.addMethod("isNull", function (value, element) {
        var flag;
        if (/\s+/.test(value)) {
            flag = false;
        } else {
            flag = true;
        }
        return flag;
    }, "请勿输入空格字符");

    // 是否
    jQuery.validator.addMethod("YesOrNo", function(value, element) {
        var flag;
        if (!(/\s+/.test(value)) && (value == "是" || value == "否") || value.length == 0) {
            flag = true;
        } else {
            flag = false;
        }
        return flag;
    }, "请勿输入“是”或“否”以外的字符");

    // 有无
    jQuery.validator.addMethod("DoesHave", function (value, element) {
        var flag;
        if (!(/\s+/.test(value)) && (value == "有" || value == "无") || value.length == 0) {
            flag = true;
        } else {
            flag = false;
        }
        return flag;
    }, "请勿输入“有”或“无”以外的字符");

    //不允许选择请选择项
    jQuery.validator.addMethod("notspecial", function (value, element, param) {
        var flag = false;
        if ($("#Country").find("option:selected").text() == "中国") {
            if ($(element).find("option:selected").text() !== "--请选择--") {
                flag = true;
            }
        }
        return flag;
    }, $.validator.format("不允许选择请选择项"));
});

function _flag(value, flag, param) {
    if (parseInt(value) > 0) {
        if(("" + parseInt(value)).length> param){
           flag = false;
        } else {
            if (/^(?!0+(?:\.0+)?$)(?:[1-9]\d*|0)(?:\.\d{0,2})?$/.test(value)) {
                flag = true;
            } else {
                flag = false;
            }
        }
    } else {
        flag = false;
    }
    return flag;
}