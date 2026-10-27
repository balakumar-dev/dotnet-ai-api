# JWT Authentication

JWT authentication works by issuing a signed JSON Web Token after a successful login.

In ASP.NET Core the typical flow is:
1. The user posts credentials to a login endpoint.
2. The API validates the user and creates a JWT that contains claims such as user id and roles.
3. The client sends the token on later requests in the Authorization header as Bearer.
4. Authentication middleware validates the signature, issuer, audience, and expiry.

The token is self-contained. The API does not need to query the database on every request just to know who the user is, although you may still check a denylist for revoked tokens.
