This repository hosts the code that runs [Cold-Brewed DevOps](https://www.jamesqmurphy.com), my personal blog site.

## Written in C# using ASP.NET Core 3.1

You're here anyway... go ahead and clone this repo!  If you've got Visual Studio 2019, you can build and run a local copy of the website.  There are two locally-defined accounts to play with (**admin@local** and **user@local**, both with passwords `abcde`), so you can see how ASP.NET Cookie Authentication works.

## Built and Deployed By Azure DevOps Services

Friends don't let friends right-click publish.  Check out the contents of the `/build` folder, then head on over to [Azure DevOps Services](https://dev.azure.com/jamesqmurphy/www-jamesqmurphy-com/_build?definitionId=5&_a=summary) to see the build pipeline in action.  There's also a [release pipeline](https://dev.azure.com/jamesqmurphy/www-jamesqmurphy-com/_release?_a=releases&definitionId=1) set up that deploys the builds to AWS.

## Hosted on Amazon Web Services

Curious to see the details?  Check out the diagram on [my About page](https://www.jamesqmurphy.com/home/about) for the most up-to-date version.  Also check out [my blog post](https://www.jamesqmurphy.com/blog/2019/06/brewing-the-blog-4) where I explain how to publish a site to AWS, step-by-step.
