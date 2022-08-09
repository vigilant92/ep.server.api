The project is build on .net 5. Please follow the steps below to check it locally:

1. Under the ep.server.api folder there is the webapi. It has only one two endpoints ["api/albums"] and ["api/albums/{userId}"]. Both are Get Method
   By calling the endpoints you will get required albums
   to run the project, navigate to ep.server.api on cmd.

   cd "./ep.server.api"
   dotnet run
  
   it will listen to port 5001.
   on browser you can get
   https://localhost:5001/swagger/index.html
   or from postman check https://localhost:5001/api/albums

2. There are some example of unit test on backend on ep.server.api.test project.
   to test, navigate to the test folder on cmd
   
   cd "./ep.server.api.test"
   dotnet test

This is a sample project. In real life, we need to implement authentication and authorization. Also we need to apply pagination and sorting.
Also, as the external api is not changing data regularly, a caching need to be added to improve performance.

Please contact me if anything needed.

Regards,
Tanvir
tanvirsamad.uk@gmail.com