using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PizzaStoreV3.Model;
public class Pizza
{
	[property: Required]
	[property: Description("The unique identifier for the pizza")]
	public int Id { get; set; }
	[property: Description("The name of the pizza")]
	[property: MaxLength(120)]
	public string? Name { get; set; }
	[property: Description("Description of the pizza")]
	public string? Description { get; set; }
}

