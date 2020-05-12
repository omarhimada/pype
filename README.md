# Pype
Pype is a generic API utility which aims to expedite implementation with third-parties. Includes ILogger support

**https://www.nuget.org/packages/FloPype/**

### GET example:
```` C#
// Make a fitting to get animals from a zoo API
Fitting animalsFitting = new Fitting
{
  ApiBasePath = "https://the-zoo.com",
  RequestSuffix = "/api/animals/123",
  ContentType = "application/json",
  Method = "GET"
};

// Send the request asynchronously 
FittingResponse<Animal> response = await animalsFitting.SendRequest<Animal>();

// Check the status of the response
switch (response.Status.Health) { ... }

// Do something with the result
Animal myAnimal = response.Result;
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
FittingResponse<Animal> response = await createAnimalFitting.SendRequest<Animal>();

// Check the status of the response
switch (response.Status.Health) { ... }

// Do something with the result
Animal myNewAnimal = response.Result;
````

### 'OpenFaucet' to return a Stream
*Use this if you don't want to hold the response in memory for performance reasons.*
````C#
using (Stream stream = await _fitting.OpenFaucet())
using (StreamReader sr = new StreamReader(stream))
using (JsonReader reader = new JsonTextReader(sr))
{
    JsonSerializer serializer = new JsonSerializer();

    // * For performance: read the JSON response from a stream
    FittingResponse<SomeBigThing> response = serializer.Deserialize<FittingResponse<SomeBigThing>>(reader);

    if (response.Status.Health == FittingResponseStatusHealth.Good)
    {
      return response.Result;
    }
}
````
