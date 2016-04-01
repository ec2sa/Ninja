
function DeleteFile(fileID) {
	$.ajax({
		url: root_path + 'Chatroom/' + $('#dialogChat').data('roomId') + '/DeleteFile',
		type: "POST",
		data: { PIN: $('#dialogChat').data('pin'), ID: fileID }
	})
        .done(function (data) {
        	AlertMessage(data, false);
        });

	$('#FileID_' + fileID).remove();
};

function DownloadFile(id) {
	window.open(root_path + 'Chatroom/' + $('#dialogChat').data('roomId') + '/DownFile' + '?PIN=' + $('#dialogChat').data('pin') + '&ID=' + id);
}

$(function () {
    var progressbar = $("#progressbar"),
		progressLabel = $(".progress-label");

	progressbar.progressbar({
		value: false,
		change: function () {
		    progressLabel.text(progressbar.progressbar("value") + "%");
		    $('#progressbar > div').attr('style', 'background: #000 url(' + root_path + 'Content/css/images/' + LOCALIZED.Culture_ + '/time_.jpg) left center !important');
		},
		complete: function () {
			progressLabel.text("Czas minął!");
			$("#dialogChat").dialog('close');
		},
		done: function () {
			progressLabel.text("Czas minął!");
			$("#dialogChat").dialog('close');
		}
	});

	$("#dialogChat").dialog({
		autoOpen: true,
		modal: true,
		minWidth: 660,
		minHeight: 450,
		width: 660,
		height: 450,
		show: {
			effect: "blind",
			duration: "fast"
		},
		hide: {
			effect: "blind",
			duration: "fast"
		},
		beforeClose: function (event, ui) {
			$('#runAwayButton').click();
		}
	});

	$('#PINDialog').dialog({
		modal: true,
		autoOpen: false,
		buttons: {
			Ok: function () {
				$(this).dialog("close");
			}
		}
	});

	$('#ChatWindow').css("height", (parseInt($("#chat_room").css("height")) - 5).toString() + 'px');
	$('#dialogChat').dialog('option', 'title', 'NINJA - ' + $('#dialogChat').data('roomName'));

	$(window).resize(function () {
		$("#dialogChat.ui-dialog-content").dialog("option", "position", ['center', 'center']);
		if ($("#dialogChat.ui-dialog-content").dialog("option", "width") > document.body.offsetWidth - 20) {
			$("#dialogChat.ui-dialog-content").dialog("option", "minWidth", 100);
			$("#dialogChat.ui-dialog-content").dialog("option", "width", document.body.offsetWidth - 20);
		}

		if ($("#dialogChat.ui-dialog-content").dialog("option", "height") > document.body.offsetHeight - 20) {
			$("#dialogChat.ui-dialog-content").dialog("option", "minHeight", 100);
			$("#dialogChat.ui-dialog-content").dialog("option", "height", document.body.offsetHeight - 20);
		}

		$("#scroll_users").css("height", (parseInt($("#chat_room").css("height")) - 100).toString() + 'px');
	});

	$("#dialogChat.ui-dialog-content").on('dialogresize', function () {
	    $('#ChatWindow').css("height", (parseInt($("#chat_room").css("height")) - 5).toString() + 'px');
	    $("#scroll_users").css("height", (parseInt($("#chat_room").css("height")) - 100).toString() + 'px');
	});

	$("#dialogJoin").dialog({
		autoOpen: false,
		modal: true,
		minWidth: 480,
		show: {
			effect: "blind",
			duration: "fast"
		},
		hide: {
			effect: "blind",
			duration: "fast"
		}
	});

	if ($("#PINVerified").val() != 'true') {
		$("#dialogJoin").dialog("open");
		clearInterval(intervalPullID);
	}

	var intervalPullID = setInterval(pullCall, 1500);

	$("#buttonAddSlot").click(function () {
		ExtendTime();
		$.ajax({
			url: root_path + 'Chatroom/' + $('#dialogChat').data('roomId') + '/AddSlot',
			type: "POST",
			data: { PIN: $('#dialogChat').data('pin') },
			success: function (data) {
				AlertMessage(data, false);
			},
			error: function (data) {
			},
			complete: function (data) {
			},
			done: function (data) {
				if (data != null) {
					$('#alert').append("<div class='error-message'>" + data + "</div>");
					$("#alert").dialog("open");
				}
			}
		});
	});

	$('#FileStream').change(function () {
	    if ($('#FileStream').val() != "") {
			$('#UploadForm').ajaxSubmit({
				type: "POST",
				success: (function (data) {
				    AlertMessage(data, false);
				}),
				error: (function (data) {
				    AlertMessage(data, false);
				    alert('Za duży plik');
				})
			});
			$('#FileStream').val('');
		}

		ExtendTime();
	});

	$("#FileUpload").click(function () {
		$('#FileStream').click();
	});

	$("#statusDiv").click(function () {
		ExtendTime();
		$.ajax({
			url: root_path + 'Chatroom/' + $('#dialogChat').data('roomId') + '/ResetIncidents',
			type: "POST",
			data: { PIN: $('#dialogChat').data('pin') }
		})
			.done(function (data) {

			});
	});

	function ExtendTime() {
		$.ajax({
			url: root_path + 'Chatroom/' + $('#dialogChat').data('roomId') + '/ExtendTime',
			type: "POST",
			data: { PIN: $('#dialogChat').data('pin') }
		})
			.done(function (data) {

			});
	}

	$("#buttonExtend, .buttonExtend").click(ExtendTime);

	$('#UsersDiv').on('click', '.KickButton', function () {
		ExtendTime();
		$.ajax({
			url: root_path + 'Chatroom/' + $('#dialogChat').data('roomId') + '/Kick',
			type: "POST",
			data: { PIN: $('#dialogChat').data('pin'), UserId: this.value },
			success: function(data) {
				AlertMessage(data, false);
			}
		}).done(function (data) {
			if (data != null && !data.Messages) {
				$('#alert').append("<div class='error-message'>" + data + "</div>");
				$("#alert").dialog("open");
			}

		});

	});

	$('#ChatMessage').keydown(function (event) {
		ExtendTime();

		var keypressed = event.keyCode || event.which;
		if (keypressed == 13) {
			$.ajax({
				url: root_path + 'Chatroom/' + $('#dialogChat').data('roomId') + '/Push',
				type: "POST",
				data: { PIN: $('#dialogChat').data('pin'), Message: '"' + $('#ChatMessage').val() + '"' }
			})
				.done(function (data) {
					AlertMessage(data, true);
				});

			$('#ChatMessage').val("");
		}
	});

	function NickColor(name) {
		if (name == "SYSTEM") return "#EEFFCC";

		var colors = ["#FF0000", "#33FF33", "#6666FF", "#FF66FF", "#99FFCC", "#CC99FF", "#CCFF99", "#FFFF00"];

		var chars = name.split('');
		var sum = 0;

		for (var i in chars) {
			sum += chars[i].charCodeAt();
		}
		var index = (sum % colors.length);
		return colors[index];
	}


	function AlertMessage(data, scroll) {
		if (data != null && data != "") {
			if (data.Access == true) {
			    $.each(data.Messages, function (i, item) {
			        item.text = item.text.replace(/\[(\w+)\]/g, function (s, key) {
			            return LOCALIZED[key] || s;
			        });
                    
                    $('#ChatWindow').append('<p class="chatmessage chatmessage_' + item.type + '">' + item.text + '</p>');
                    $('#ChatWindow').scrollTop($('#ChatWindow').prop('scrollHeight'));
                    $('#buttonAddSlot').removeAttr('disabled');
				});
				var usersTable = $("<table id='UsersTable'></table>");

				$('#UsersDiv').html(usersTable);

				var userCount = Object.keys(data.Users).length;
				$.each(data.Users, function (i, item) {
					//TODO
					if ($("body > #userSent_" + item.Pin).length == 0 && userCount > 0) {
						$("body").append("<div style='display:none' id='userSent_" + item.Pin + "'></div>");
						if (userCount > 1) {
							if (item.Pin != null) CallPinDialog(item.Pin);
						}
					}


					var row = $("<tr></tr>");

					if ($('#dialogChat').data('isOwner') == 'True') {

						row.append(
							$('<td class="usertr" data-userpin="'
								+ item.Pin
								+ '" onclick="$(\'#PINDialog\').dialog(\'open\').text(\'PIN użytkownika: '
								+ item.Pin
								+ '\');" style="cursor:pointer;'
								+ (item.Nick != null ? ' color:' + NickColor(item.Nick) + ';' : '')
								+ ' title="' + item.Pin + '">')
							.text(item.Nick == null ? '[PIN]' : item.Nick)
						);

						if (item.Pin == $('#dialogChat').data('pin')) {
							row.append($('<td>').text());
						} else {
							row.append($('<td>').html("<button class='KickButton' value='" + item.SlotID + "'>x</button>"));
						}
					} else {
						if (item.Nick != null) row.append($('<td class="usertr" data-userpin="' + item.Pin + '" ' + (item.Nick != null ? ' style="color:' + NickColor(item.Nick) + ';"' : '') + '>').text(item.Nick));
					}
					usersTable.append(row);


				});
			} else {
				$('#ChatWindow').val('');
				if (data.Access == false) {
					$('#alert').append("<div class='error-message'>" + data.AccessMessage + "</div>");
					$("#alert").dialog("open");
				}
			}
			if (data.File) {
				var file = data.File.replace(/[!\"#$%&'\(\)\*\+,\.\/:;<=>\?\@\[\\\]\^`\{\|\}~]/g, '');
				file = file.replace(/\ /g, '');
				$('#Files').append("<tr id=\"FileID_" + data.FileID + "\"><td style=\"cursor:pointer;\" class=\"file\" id=\"" + file + "\"><span onclick=\"DownloadFile(" + data.FileID + ")\">" + data.File + "</span></td><td><button onclick=\"DeleteFile(" + data.FileID + ");\" data-fileid=\"" + data.FileID + "\" id=\"DelFile_" + data.FileID + "\" class=\"KickButton\" style=\"cursor:pointer;\" name=\"Usuń\" value=\"x\">x</button></td></tr>");

				//document.getElementById("DelFile_"+data.FileID).addEventListener("click", Delete(data.FileID), false);

				//$('#UploadForm').hide();


				//$('#fileID').click(function () {

				//});
			}
			if (data.Delete === true) {
				//$('#Files').children("button[data-fileid='"+data.FileID+"']").remove();
				$('#FileID_' + data.FileID).remove();
			}
			//if (data.Messages != null && data.Messages.length > 0)
			//$('#ChatMessage').val('');

			if (scroll == true) {
				$('#ChatWindow').scrollTop($('#ChatWindow').prop('scrollHeight'));
			}
		}
	};


	function CallPinDialog(pin) {
		var msg = "";
		var address = $('span.chat-header.address').html();

		msg += "<p>"+ LOCALIZED.ChatWindow_AddMember_Instructions +": </p>";
		msg += "<p><i><small><a style='color: #587F8D;' href='" + address + "'>" + address + "</a></small></i></p>";
		msg += "<p>" + LOCALIZED.ChatWindow_AddMember_Pin + ": " +pin + "</p>";

		$('#PINDialog').dialog('open').html(msg);
	};
	function pullCall() {
	    $('.ui-dialog-titlebar-close, .ui-corner-all, ui-icon ui-icon-closethick, [role="button"]').attr('tabindex', '-1');

	    $.ajax({
			url: root_path + 'Chatroom/' + $('#dialogChat').data('roomId') + '/Pull',
			type: "POST",
			data: { PIN: $('#dialogChat').data('pin') }
		}).done(function (data) {
			AlertMessage(data, false);

			if (data.Access === false) {
				clearInterval(intervalPullID);
			}

			if (data.Incidents != null) {
				if (data.Incidents.length == 0) {
					$('#statusDiv').css("background-image", "url(" + root_path + "Content/css/images/lights_green.png)");
					$('#statusDiv').attr('title', LOCALIZED.ChatWindow_SecurityLevel_1);
				} else
					if (data.Incidents.length == 1) {
						$('#statusDiv').css("background-image", "url(" + root_path + "Content/css/images/lights_yellow.png)");
						$('#statusDiv').attr('title', LOCALIZED.ChatWindow_SecurityLevel_2);

						var retstr = "<p style='color: darkorange;'>" + data.Incidents[0].Description + "</p>";

						//$('#statusDiv').tooltip({
						//    content: retstr
						//});


					} else if (data.Incidents.length > 1) {
						$('#statusDiv').css("background-image", "url(" + root_path + "Content/css/images/lights_red.png)");
						$('#statusDiv').attr('title', LOCALIZED.ChatWindow_SecurityLevel_3);

						var retstr = "";
						for (i = 0 ; i < data.Incidents.length; i++)
							retstr += "<p style='color: red;'>" + data.Incidents[i].Description + "</p>";

						//$('#statusDiv').tooltip({
						//    content: retstr
						//});
					}
			}

			progressbar.progressbar("value", data.Expiration);
			if (data.Access == false) {
				$("#dialogChat").dialog('close');
			}
		});
	};
});


$("div#footer").show();
$('body').addClass('chatbody');
$('html').addClass('chatbody');

$('#chat_room').css({ 'min-height': (window.innerHeight - 50).toString() + 'px' });



if (navigator.userAgent.search("MSIE") >= 0 || navigator.userAgent.indexOf('Trident/') >= 0 || navigator.userAgent.indexOf('Edge/') >= 0) {
    $('document').ready(function () {
        $('#ChatWindow').css('height', 225);
        $('td#chat_room').css('height', 220);
        $('td#chat_room').attr('style', 'height: 220px;');
        $('#dialogChat').css('height', 330);
        $("#dialogChat").dialog("option", "height", 430);
    });
}