#WebPagesSugar
(version 0.5 work in progress)

The RazorShoulds is using RazorTemplates for rendering and RazorTemplates is tested with RazorShoulds :)

##RazorTemplates - nice convention based templating

###Basic usage:
  
	@{
		var tmpl = new Templates();
		dynamic dynamicTemplate = tmpl;
		var someObject = new MyNameValueType{ Name = "Foo", Value = "Bar" };
	  }

	@tmpl.Render("CustomHeader", someObject.Name)
>>renders /templates/CustomHeader.cshtml

	@tmpl.Render(someObject);
>>renders /templates/MyNameValueType.cshtml

	@dynamicTemplate.CustomHeader(someObject.Name)
>>renders /templates/CustomHeader.cshtml

##RazorShoulds - tiny web based testrunner with approval tests features

1. Add your .cshtml files to a /tests folder (like in the samples project) give them descriptive names (can have spaces)
2. And open the /tests url to start the runner. 
3. The testrunner renders each .cshtml in the folder and "says approval files missing". The testrunner saves the 
rendered files the /rendered/ folder.
4. Check the results, optionally edit them, and move to the /approved/ folder. 
5. Change your cshtml's to get the right results.
6. Open the /tests url again to re-run testrunner.