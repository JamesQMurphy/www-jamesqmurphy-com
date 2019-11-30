I've always wanted to build a living, breathing site with Continuous Integration/Continuous Deployment tools -- and blog about it.  So why not build the blog website itself?  Here are the main elements of the CI/CD process:

# Source Control Stored In Github
https://github.com/JamesQMurphy/www-jamesqmurphy-com

When it comes to source control for a public project, GitHub is still the obvious choice.  My
branching strategy is more-or-less [GitHub Flow](https://guides.github.com/introduction/flow/index.html):

1. Create feature branch named `features\new-feature`
2. Update version number in `azure-pipelines.yml` (see [my post on build numbering](/blog/2019/08/build-numbering))
3. Add commits
4. Build and deploy to **DEV** environment, if necessary
5. Open pull request against `master`
6. Merge to `master`, which triggers the production build

Sometimes, if I'm working on a bigger, long-running release, I'll create a branch named `releases\X.Y.Z` to serve as an intermediate
checkpoint.  Pushes to release branches automatically get built and deployed to **DEV**.

# Built and Deployed By Azure DevOps Services
https://dev.azure.com/jamesqmurphy/www-jamesqmurphy-com

The build tools available with Azure DevOps Services are excellent -- and essentially free for
public projects.  [Builds](https://dev.azure.com/jamesqmurphy/www-jamesqmurphy-com/_build?definitionId=5)
are automatically triggered when commits are pushed to either the
`master` branch or a branch that starts with `releases/`.  However, *any* branch can be built
by manually triggering a build from within Azure DevOps Services.

I also have an Azure DevOps Release Pipeline named [Deploy to AWS](https://dev.azure.com/jamesqmurphy/www-jamesqmurphy-com/_release?_a=releases&definitionId=1).  Every build, except for pull-request builds, will trigger this pipeline.  If
the build came from the `master` branch, then the pipeline deploys to the **Staging and Production** environments; otherwise,
it deploys to **DEV**.

# Hosted On Amazon Web Services
https://aws.amazon.com/

The website is hosted on AWS, and takes advantage of several AWS services:

![Diagram of JamesQMurphy.com website using AWS Services like API Gateway, Lambda Functions, S3 Storage, CloudWatch Logs, SQS, SES, and DynamoDb](JamesQMurphy-AWS-Diagram.png){.mx-auto .d-block}

At its center, the website is ASP.NET Core 2.2 MVC application, hosted inside an
[AWS Lambda Function](https://aws.amazon.com/lambda/) and
exposed to the outside world via [AWS API Gateway](https://aws.amazon.com/api-gateway/).  Requests for
static content, such as images, style sheets, and JavaScript files, are forwarded to [AWS S3](https://aws.amazon.com/s3/).
The site uses [AWS DynamoDB](https://aws.amazon.com/dynamodb/) for data storage.  Sending email is
accomplished using [AWS SES](https://aws.amazon.com/ses/) protected behind an [AWS SQS Queue](https://aws.amazon.com/sqs/).

# About Me

I'm JamesQMurphy and I've been doing DevOps since before they've been calling it DevOps --
which isn't *that* long ago. I've loved coding since I was eleven years old, but I also
developed a love of the *process* of creating software.  It's also part of why I studied
Chemical Engineering at college; the two disciplines do share a connection in that arena.

My first real software job was working on a shrink-wrapped software package, as part of a
team of four developers.  This was *way* back in 1995, so Windows 3.1 was still the primary
operating system of the day (most organizations didn't adopt Windows 95 until the next
couple of years).  In addition to writing the software package's calculation engine, I also
developed the installation package (using Wyse) and helped introduce version control
(Visual SourceSafe).  I would be the one to build the software -- always taking the latest
version from SourceSafe, and always using the same batch file -- before creating the 
installation package and handing the diskettes to my manager, who inserted them into his
diskette duplicating machine.  (It even printed the labels!)  Since those days, I've worked
at many different types of companies, in all sorts of industries, but I always gravitated
to the "build and release" side of things, mostly because it was a chance to relive my
chemical engineering days by building a "software factory".

I think I first became aware of the "DevOps" term when I was Googling for stories between
developers and operations.  At the time, I worked for a software-as-a-service company, and
relations between the development and the operations teams were strained, to say the least.
The developers always claimed that the operations team "messed things up", and the Ops
guys constantly complained about the developers blaming all of the software problems on
them.  (And they both had a point, for what it was worth.)  By some ironic twist of fate,
I was relocated into the same part of the office as the ops team, and I got to know them.
I also started to overhear some of their conversations, and before long, I started to see
things from their point of view.  I asked questions. I listened.  I *learned*.

It wasn't long before I became a sort-of emissary between the development and the operations
team.  When the developers asked me to troubleshoot a "network connection problem," I
would actually strive to figure out if it was a DNS resolution problem or a routing problem.
The ops guys, who were used to hearing "hey it's broken, fix it," were much more
receptive to questions like, "When I run NSLOOKUP I get this IP address, shouldn't it be
that IP address?"

I had become a DevOps engineer, and I didn't even know it at the time.
