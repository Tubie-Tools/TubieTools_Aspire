using Microsoft.Extensions.Logging;

namespace TubieTools_Aspire.Tests;

public class Puzzles
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    [Test]
    public async Task TestZebraAndWater()
    {
        var houses = new List<House>();
        houses.Add(new House("red", "englishman", "", "",""));
        houses.Add(new House("", "spaniard", "dog", "",""));
        houses.Add(new House("", "", "", "coffee", ""));
        houses.Add(new House("", "ukranian", "", "tea", ""));
        

        var sublist = new List<House>();
        sublist.Add(new House("ivory", "", "", "", ""));
        sublist.Add(new House("green", "", "", "", ""));

        houses.Add(new House("", "", "snail", "", "old golds"));
        houses.Add(new House("yellow", "", "", "", "kools"));


        //H1,H2,H3,H4,H5 house array to contain all members of the puzzle, then assign the properties to each house based on the clues given in the puzzle. 
        var h1 = houses[0];
        var h2 = houses[1];
        var h3= new House("", "", "", "milk", ""); 
        var h4 = houses[2];
        var h5 = houses[3];
        var hQ = new House("", "norwegian", "", "", ""); //hrest? /add to list




        Member memberArray = new Member();
        memberArray.Houses = houses;

        Assert.IsNotNull(houses);
    }
}

internal class Member
{
    public List<House> Houses { get; set; } = new List<House>();
}

internal class House
{
    public House(string color, string ethnicity, string animal, string drink,  string cigarettes)
    {
        Color = color;
        Ethnicity = ethnicity;
        Animal = animal;
        Drink = drink;
        Cigarettes = cigarettes;
    }

    public string Color { get; }
    public string Ethnicity { get; }
    public string Drink { get; }
    public string Animal { get; }
    public string Cigarettes { get; }
}