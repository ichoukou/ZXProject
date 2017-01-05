/**
* author:ZhengXu,JinLi
* version:0.0.1
* 云房数据--前端产品设计部开发+维护
* learen,help,dev文件夹为开发环境，dis为发布环境。
*/
;(function ($) {
    var choosearea = function (options) {
        this.set = $.extend({
            selectDomId: {
                province: "a",
                city: "b",
                county: "c"
            },
            data: null,
            initAreaIds: {
                province: 4,
                city: 0,
                county: 0
            },
            eventInterface: {
                renderProvinceList: function (list, selectedId) {
                    this.jq_province.empty().append($(this.getListOptionsHtml(list, selectedId, "--请选择--")));
                },
                renderCityList: function (list, selectedId, isInit) {
                    var city = this.jq_city;
                    isInit = typeof (isInit) == "undefined" ? false : true;
                    city.empty().append($(this.getListOptionsHtml(list, selectedId, "--请选择--")));
                },
                renderCountyList: function (list, selectedId, isInit) {
                    var optionsHtml = this.getListOptionsHtml(list, selectedId, "--请选择--");
                    var county = this.jq_county;
                    isInit = typeof (isInit) == "undefined" ? false : true;
                    county.empty().append($(optionsHtml));
                },
                onchanged: function (cityId) {

                }
            }

        }, options);
        this.provinceList = [];
        this.cityList = [];
        this.countyList = [];
        this.jq_province = $("#" + this.set.selectDomId.province);
        this.jq_city = $("#" + this.set.selectDomId.city);
        this.jq_county = $("#" + this.set.selectDomId.county);
        this._init();
    };
    choosearea.prototype = {};
    choosearea.fn = choosearea.prototype;
    choosearea.fn._init = function () {
        if (this.set.data == null) {
            return;
        };
        this._setAreaList();
        this._initRender(this.set.initAreaIds.province, this.set.initAreaIds.city, this.set.initAreaIds.county);
        this._initEvents();
    };
    //设置地区列表
    choosearea.fn._setAreaList = function () {
        this.provinceList = this.set.data.provinceList;
        this.cityList = this.set.data.cityList;
        this.countyList = this.set.data.countyList;
    };

    //初始化渲染
    choosearea.fn._initRender = function (provinceId, cityId, countyId) {
        this.set.eventInterface.renderProvinceList.call(this, this.provinceList, provinceId);
        var cityList = this.cityList["city_" + provinceId] || [];
        this.set.eventInterface.renderCityList.call(this, cityList, cityId, true);
        var countyList = this.countyList["city_" + cityId] || [];
        this.set.eventInterface.renderCountyList.call(this, countyList, countyId, true);
    };

    //渲染列表
    choosearea.fn.getListOptionsHtml = function (list, selectedId, firstTips) {
        firstTips = firstTips || "";
        var selectedAttr = selectedId == 0 ? " selected='selected'" : "";
        var optionsHtml = firstTips != "" ? "<option  " + selectedAttr + ">" + firstTips + "</option>" : "";
        var sel = $("#province");
        if (typeof (list) != "undefined") {
            $.each(list, function (i, city) {
                var selAttr = selectedId == city.id ? " selected='selected'" : "";
                optionsHtml += "<option value='" + city.id + "' " + selAttr + ">" + city.name + "</option>";
            });
        };
        return optionsHtml;
    };
    //初始化事件
    choosearea.fn._initEvents = function () {
        var province = this.jq_province;
        var city = this.jq_city;
        var county = this.jq_county;
        var _this = this;
        province.change(function () {
            var id = parseInt($(this).val());
            var cityList = _this.cityList["city_" + id] || [];
            _this.set.eventInterface.renderCityList.call(_this, cityList, 0);
            _this.set.eventInterface.renderCountyList.call(_this, [], 0, false);
            _this.set.eventInterface.onchanged.call(this, id);
        });

        city.change(function () {
            var id = parseInt($(this).val());
            var countyList = _this.countyList["city_" + id] || [];
            _this.set.eventInterface.renderCountyList.call(_this, countyList, 0, false);
            _this.set.eventInterface.onchanged.call(this, id);
        });
    };
    $.choosearea = choosearea;
})(jQuery);
/* yundata--grunt 最后修改于： 2016-03-24 */