# NDatabase
Fork of NDatabase from CodePlex

NDatabase is an object database (also known as a Document Database). It's fast and easy to use. 

The website can be found here: http://ndatabase.wixsite.com/home


## How to persist an object with NDatabase
	// Create the instance be stored
	var sport = new Sport("volley-ball");

	// Open the database
	using (var odb = OdbFactory.Open("test.db"))
	{
		// Store the object
		odb.Store(sport);
	}

## How to retrieve object from NDatabase
	// Open the database
	using (var odb1 = OdbFactory.Open("test.db"))
	{
		//                IQueryable<Sport>
		var sports = odb1.QueryAndExecute<Sport>();

		// code working on sports list
	}

	//The Sport class definition
	public class Sport
	{
		public Sport(string name)
		{
			Name = name;
		}

		public string Name { get; set; }

		public override string ToString()
		{
			return Name;
		}
	}