# FloPype
FloPype is a generic API utility which aims to expedite implementation with third-parties. Includes ILogger support

**https://www.nuget.org/packages/FloPype/**

#### Simple GET example:
```` C#
// Make a fitting to get animals from a zoo API
Fitting<AnimalService> animalsFitting = new Fitting<AnimalService>
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
#### Simple POST example:
```` C#
// Make a fitting to create a new animal
Fitting<AnimalService> createAnimalFitting = new Fitting<AnimalService>
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
#### 'OpenFaucet' to return a Stream
*Use this if you don't want to hold the response in memory for performance reasons.*
````C#
using (Stream stream = await _fitting.OpenFaucet())
using (StreamReader sr = new StreamReader(stream))
using (JsonReader reader = new JsonTextReader(sr))
{
    JsonSerializer serializer = new JsonSerializer();

    // * For performance: read the JSON response from a stream
    FittingResponse response = serializer.Deserialize<FittingResponse>(reader);

    if (response.Status.Health == FittingResponseStatusHealth.Good)
    {
      return response.Result.ToObject<List<Offer>>();
    }
}
````
