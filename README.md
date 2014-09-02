MonadicIT
=========

This project is part of my Bachelor's thesis and serves as a case study for how monads can be utilized 
in projects where benefit or usage are not immediately obvious.

Also, it seemed like a fun project to apply my knowledge gained in an information theory lecture.
In trying my best to visualize each layer as my intuition dictates, I hope to help people having a 
hard time to grasp this and speeding up their building up of intuition.

Of course this is far from complete as this project to date only serves as a proof of concept for thesis.
Yet extending it with different kinds of encoding techniques could be worthwhile and fun. 
But, you know, I got only a finite amount of time to spare :)

How To Build
============

Development was done with VS 2013, although VS 2012 could possibly work, too. I had .NET 4.5 installed, but .NET 4.0 should be fine too. Off the top of my head, I use C# 5 features. NuGet should download the referenced packages on build by itself.

How To Run
==========

A Windows installation with .NET 4.0 Client Profile is required. Then grab a version from the [CI server](https://ci.appveyor.com/project/sgraf812/monadicit), preferrably a release from the master branch and start playing around.

What is This?
=============
Well, I tried to embed some short documentation inside the app. I will probably elaborate on what can be done with it at a later point (aka TODO: clean this up). That said, upon starting you find yourself with a sketch of a 4-tiered transmission system. Pro tip: Adjust the source symbol rate in the source model to bring life into the picture, adjust symbol probabilities, change channel error rate and channel coder.

License
=======

The source code is licensed under the MIT license.
