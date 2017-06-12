function AjaxRequest(url, data, success, error)
{
	//alert('Request send to url:' + url);
	$.ajax
	(
		{
			url: url,
			type: 'GET',
			cache: false,
			async: false,
			data: data,
			success: function (result) 
			{	
				success(result);
			},
			error: function (e1, e2, e3) {
			    console.log(e1);
			    console.log(e2);
			    console.log(e3);

			}
    	}
	);	
}