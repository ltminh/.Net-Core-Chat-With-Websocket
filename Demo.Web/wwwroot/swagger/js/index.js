window.JSConfig = JSON.parse('{"SwaggerEndpoints":[{"Url":"/swagger/v1/swagger.json","Description":"My API V1"}],"BooleanValues":["false","true"],"DocExpansion":"list","SupportedSubmitMethods":["get","post","put","delete","patch"],"OnCompleteScripts":[],"OnFailureScripts":[],"ShowRequestHeaders":false,"JsonEditor":false,"OAuth2ClientId":"your-client-id","OAuth2ClientSecret":"your-client-secret-if-required","OAuth2Realm":"your-realms","OAuth2AppName":"your-app-name","OAuth2ScopeSeparator":" ","OAuth2AdditionalQueryStringParams":{}}');

hljs.configure({
    highlightSizeThreshold: 5000
});

// Pre load translate...
if (window.SwaggerTranslator) {
    window.SwaggerTranslator.translate();
}

window.swaggerUi = new SwaggerUi({
    url: JSConfig.SwaggerEndpoints[0].Url,
    validatorUrl: JSConfig.ValidatorUrl,
    dom_id: "swagger-ui-container",
    booleanValues: JSConfig.BooleanValues,
    supportedSubmitMethods: JSConfig.SupportedSubmitMethods,
    onComplete: function (swaggerApi, swaggerUi) {
        if (typeof initOAuth === "function") {
            initOAuth({
                clientId: JSConfig.OAuth2ClientId,
                clientSecret: JSConfig.OAuth2ClientSecret,
                realm: JSConfig.OAuth2Realm,
                appName: JSConfig.OAuth2AppName,
                scopeSeparator: JSConfig.OAuth2ScopeSeparator,
                additionalQueryStringParams: JSConfig.OAuth2AdditionalQueryStringParams
            });
        }

        if (window.SwaggerTranslator) {
            window.SwaggerTranslator.translate();
        }

        _.each(JSConfig.OnCompleteScripts, function (script) {
            $.getScript(script);
        });
    },
    onFailure: function (data) {
        log("Unable to Load SwaggerUI");

        _.each(JSConfig.OnFailureScripts, function (script) {
            $.getScript(script);
        });
    },
    docExpansion: JSConfig.DocExpansion,
    jsonEditor: JSConfig.JsonEditor,
    defaultModelRendering: 'schema',
    showRequestHeaders: JSConfig.ShowRequestHeaders,
    showOperationIds: false
});

function addToken(accessToken) {
    var bearerToken = 'Bearer ' + accessToken;

    window.swaggerUi.api.clientAuthorizations.add("api_key", new SwaggerClient.ApiKeyAuthorization("api_key", accessToken, "query"));
    window.swaggerUi.api.clientAuthorizations.add("Authorization", new SwaggerClient.ApiKeyAuthorization("Authorization", bearerToken, "header"));
    //window.swaggerUi.headerView.showCustom();
}

function addApiKeyAuthorization() {
    var key = encodeURIComponent($('#input_apiKey')[0].value);
    if (key && key.trim() !== "") {

        addToken(key);
        log("added key " + key);
    }
}

$('#input_apiKey').change(addApiKeyAuthorization);

$("#login").click(function () {
    var username = $('#username').val();
    var password = $('#password').val();

    loginFunc(username, password);
});

$("#username").keypress(function (e) {
    if (e.which === 13) {
        var username = $('#username').val();
        var password = $('#password').val();

        loginFunc(username, password);
    }
});

$("#password").keypress(function (e) {
    if (e.which === 13) {
        var username = $('#username').val();
        var password = $('#password').val();

        loginFunc(username, password);
    }
});

function loginFunc(username, password) {
    $('#spinner-1').show();

    $.ajax({
        url: config.usernamePassword.url,
        type: 'post',
        contenttype: 'x-www-form-urlencoded',
        data: 'username=' + username + '&password=' + password,     
        success: function (response) {
            var token = response.data.access_token;
            log("added key " + token);
            addToken(token);
            $('#spinner-1').hide();
            $('.login-success').fadeIn(400).delay(3000).fadeOut(400); //fade out after 3 seconds
        },
        error: function (xhr, ajaxoptions, thrownerror) {
            $('#spinner-1').hide();
            $('.login-error').fadeIn(400).delay(3000).fadeOut(400); //fade out after 3 seconds
        }
    });
};

$("#loginExternal").click(function () {
    var provider = $("#providerOption").val();
    var externalAccessToken = $('#externalAccessToken').val();

    loginExternalFunc(provider, externalAccessToken);
});

$("#externalAccessToken").keypress(function (e) {
    if (e.which === 13) {
        var provider = $("#providerOption").val();
        var externalAccessToken = $('#externalAccessToken').val();

        loginExternalFunc(provider, externalAccessToken);
    }
});

function loginExternalFunc(provider, externalAccessToken) {
    $('#spinner-1').show();

    var dataPost = {
        provider: provider,
        externalAccessToken: externalAccessToken
    };

    $.ajax({
        url: config.externalLogin.url,
        type: "post",
        contenttype: 'application/json',
        data: dataPost,
        success: function (response) {
            //var token = response.data.access_token;
            var token = response.data.access_token;

            log("added key " + token);
            addToken(token);
            $('#spinner-1').hide();
            $('.login-success').fadeIn(400).delay(3000).fadeOut(400); //fade out after 3 seconds
        },
        error: function (xhr, ajaxoptions, thrownerror) {
            $('#spinner-1').hide();
            $('.login-error').fadeIn(400).delay(3000).fadeOut(400); //fade out after 3 seconds
        }
    });
};

$("#loginOption").change(function () {
    var idNeedShow = $(this).val();

    $("#api_selector").hide();
    $("#loginOptionUsernamePassword").hide();
    $("#loginOptionExternal").hide();
    $("#loginSocial").hide();

    $("#" + idNeedShow).show();
});

$("#providerOption").change(function () {
    //alert($(this).val());
});

function init() {
    var selectHtml = "";

    if (config.token.active) {
        selectHtml += "<option value='api_selector'>Token</option>";
    }

    if (config.usernamePassword.active) {
        selectHtml += "<option value='loginOptionUsernamePassword' selected='selected'>Username & Password</option>";
    }

    if (config.externalLogin.active) {
        selectHtml += "<option value='loginOptionExternal'>External login</option>";

        var providerOptionHtml = "";

        for (var i = 0; i < config.externalLogin.options.length; i++) {
            providerOptionHtml += "<option value='" + config.externalLogin.options[i].providerName + "'>" + config.externalLogin.options[i].name + "</option>";
        }

        $("#providerOption").html(providerOptionHtml);
    }

    if (config.loginSocial.active) {
        selectHtml += "<option value='loginSocial'>Login Social</option>";
    }

    $("#loginOption").html(selectHtml);
    $("#api_selector").hide();
    $("#loginOptionUsernamePassword").show();
    $("#loginOptionExternal").hide();
    $("#loginSocial").hide();

};
init();

window.swaggerUi.load();

function log() {
    if ('console' in window) {
        console.log.apply(console, arguments);
    }
}