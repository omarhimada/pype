# FloPype
FloPype is a generic API utility with built-in logging, which aims to expedite implementation with third-parties.

#### GET example:
```` C#
// Make a fitting to get animals from a zoo API
Fitting animalsFitting = new Fitting
{
  ApiBasePath = "https://the-zoo.com",
  RequestSuffix = "/api/animals",
  ContentType = "application/json",
  Method = "GET"
};

// Send the request asynchronously 
FittingResponse animalsResponse = await animalsFitting.SendRequest();

// Check the status of the response
switch (animalsResponse.Status.Health) { ... }

// Do something with the result
JsonConvert.DeserializeObject<List<Animal>>(animalsResponse.Result);
````
#### POST example:
```` C#
// Make a fitting to create a new animal
Fitting createAnimalFitting = new Fitting
{
  ApiBasePath = "https://the-zoo.com",
  RequestSuffix = "/api/animals/create",
  ContentType = "application/json",
  Method = "POST"
};

// Parameters can be changed at any point so you can re-use the Fitting
createAnimalFitting.Parameters = new Dictionary<string, object>
{
  { "Name", "Zebra" },
  { "Stripes", true },
  { "Legs", 4 }
};

// Send the request asynchronously 
FittingResponse createAnimalFitting = await createAnimalFitting.SendRequest();

// Check the status of the response
switch (createAnimalFitting.Status.Health) { ... }

// Do something with the result
JsonConvert.DeserializeObject<Animal>(createAnimalFitting.Result);
````
### https://www.nuget.org/packages/FloPype/
