$(document).ready(function () {
	$('body').css('display', 'none');
	$('body').fadeIn(1000);

    setInterval(function () {

        $.ajax({
            url: '/Home/Update',
            success: function (data) {
                $("#projects").html(data);
            }
        });
        
	}, 15000);
});