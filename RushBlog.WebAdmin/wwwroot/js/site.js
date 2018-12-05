$(function () {
	$(".toggle-area").on("click", function (e) {
		e.preventDefault();
		var area = $(this).attr("data-area");
		$("#" + area).slideToggle();
		$(this).find(".fa-toggle-down, .fa-toggle-up").toggleClass("fa-toggle-down").toggleClass("fa-toggle-up");
	});

	$('.datepicker').datepicker({ autoclose: true });
});

function formatDate(jsonDate) {

	if (jsonDate === null) {
		return "";
	}
	var date = null;
	if (isFunction(jsonDate)) {
		var temp = jsonDate();
		if (temp === null) {
			return "";
		}
		date = new Date(temp);
	} else {
		date = jsonDate;
	}

	var month = date.getMonth() + 1;
	var year = date.getFullYear();
	var day = date.getDate();

	return month + "/" + day + "/" + year;
}

function formatDate2(jsonDate) {
	if (jsonDate === null)
		return "";

	if (isFunction(jsonDate))
		jsonDate = jsonDate();

	if (jsonDate === null)
		return "";

	var date = new Date(parseInt(jsonDate.substr(6)));

	var month = date.getMonth() + 1;
	var year = date.getFullYear();
	var day = date.getDate();

	return month + "/" + day + "/" + year;
}

function formatDateTime(jsonDate) {
	if (jsonDate === null)
		return "";

	if (isFunction(jsonDate))
		jsonDate = jsonDate();

	if (jsonDate === null)
		return "";

	var date = new Date(parseInt(jsonDate.substr(6)));

	var month = date.getMonth() + 1;
	var year = date.getFullYear();
	var day = date.getDate();
	var sec = date.getSeconds();

	var ampm = "AM";
	var hour = date.getHours();
	if (hour > 12) {
		hour = hour - 12;
		ampm = "PM";
	}
	var mins = date.getMinutes();

	return month + "/" + day + "/" + year + " " + hour + ":" + pad(mins, 2) + ":" + pad(sec, 2) + " " + ampm;
}

function isFunction(functionToCheck) {
	var getType = {};
	return functionToCheck && getType.toString.call(functionToCheck) === '[object Function]';
}

function getNullableBooleanValue(identifier) {
	var stringValue = $(identifier).val();
	if (stringValue === null)
		return null;

	stringValue = stringValue.trim().toUpperCase();
	if (stringValue.length === 0)
		return null;

	return stringValue === "TRUE" || stringValue === "YES" || stringValue === "1";
}

//removes an item from an array by value
Array.prototype.remove = function () {
	var what, a = arguments, L = a.length, ax;
	while (L && this.length) {
		what = a[--L];
		while ((ax = this.indexOf(what)) !== -1) {
			this.splice(ax, 1);
		}
	}
	return this;
};

function pad(num, size) {
	var s = num + "";
	while (s.length < size) s = "0" + s;
	return s;
}

function slugify(str) {
	str = str.replace(/^\s+|\s+$/g, ""); // trim
	str = str.toLowerCase();

	// remove accents, swap ñ for n, etc
	var from = "åàáãäâèéëêìíïîòóöôùúüûñç·/_,:;";
	var to   = "aaaaaaeeeeiiiioooouuuunc------";

	for (var i = 0, l = from.length; i < l; i++) {
		str = str.replace(new RegExp(from.charAt(i), "g"), to.charAt(i));
	}

	str = str
		.replace(/[^a-z0-9 -]/g, "") // remove invalid chars
		.replace(/\s+/g, "-") // collapse whitespace and replace by -
		.replace(/-+/g, "-") // collapse dashes
		.replace(/^-+/, "") // trim - from start of text
		.replace(/-+$/, ""); // trim - from end of text

	return str;
}
