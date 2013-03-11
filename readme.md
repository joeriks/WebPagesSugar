#WebPagesSugar
(version 0.5 work in progress)

The RazorShoulds is using RazorTemplates for rendering and RazorTemplates is tested with RazorShoulds

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

###Model based dynamic sugar
	@{
		dynamic tmpl = new Templates(Model);
	}
	@tmpl.BodyText
>>renders /templates/BodyText.cshtml

Everything falls back to [/shared/{templatename}, _defaultTemplate.cshtml, /shared/_defaultTemplate.cshtml] if the requested template does not exist.

In Umbraco the default template locations is /macroScripts/ and /macroScripts/shared/

##RazorShoulds - tiny web based testrunner with approval tests features

1. Add your .cshtml files to a /tests folder (like in the samples project). Give them descriptive names.
2. And open the /tests url to start the runner. 
3. The testrunner renders each .cshtml in the folder and says "approval files missing". The testrunner saves the 
rendered files for verification in the /rendered/ folder.
4. Check the results, optionally edit them, and move to the /approved/ folder. 
5. Change your cshtml's to get the right results.
6. Open the /tests url again to re-run testrunner - now it checks the results against the files in /approved/ folder.

    Sample testresults:

    Approval tests on all files in folder  
    File: Basic samples (OK)  
    File: Header as h1 (OK)  
    File: Missing template (OK)  
    File: Test without approval file (FAILED)  
    No approval file exists, check folder /rendered/ and and move to /approved/ after they have been approved  
    File: Type template (OK)