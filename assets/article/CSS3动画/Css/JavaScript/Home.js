$(function()
{
	for( i=0 ;i<7;i++)
	{
		yuan = document.createElement("div");
		yuan.className="sun";
		yuan.style.left=Math.floor(Math.random()*(500-100+1))+100+"px";
		yuan.style.top=Math.floor(Math.random()*(500-100+1))+100+"px";
		var first = document.body.firstChild;
		document.body.insertBefore(yuan, first);
	}
	
});