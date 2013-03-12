using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;



public class FakeRepository
{
    public static SampleModel GetModel()
    {
        return new SampleModel
        {
            Header = "RazorTemplates demo",
            BodyText = new HtmlString(@"<p>Dang ipsum <b>nizzle sit amizzle</b>, consectetuer sure tellivizzle.</p><p>Boofron fo velit, aliquet volutpizzle, fo shizzle doggy, gravida vizzle, crackalackin. Pellentesque egizzle doggy. Sed eros. Check out this izzle sheezy dapibizzle its fo rizzle tempizzle fizzle. Mauris pellentesque nibh izzle turpis. Dang izzle tortizzle. Doggy break it down mah nizzle. In hizzle check out this platea dictumst. Donec dapibizzle. Da bomb tellus shit, pretizzle fo shizzle, that's the shizzle ac, eleifend vitae, nunc.</p> <h2>Tellivizzle suscipizzle</h2><p> Integer sempizzle velit shit purus.</p>"),
            CustomProperty = new CustomType
            {
                Name = "Foo",
                Value = "Bar"
            }
        };
    }
}

public class SampleModel: System.Dynamic.DynamicObject
{
    public string Header { get; set; }
    public IHtmlString BodyText { get; set; }
    public CustomType CustomProperty { get; set; }

}
public class CustomType
{
    public string Name { get; set; }
    public string Value { get; set; }
}
