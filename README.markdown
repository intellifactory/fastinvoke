# IntelliFactory.FastInvoke

`IntelliFactory.FastInvoke` implements a common pattern of optimizing
repeated delegate invocations by pre-binding to the delegate.

## Copying

Code is available under Apache 2.0 license, see LICENSE.txt in source.

## Installation

Consider using the binaries hosted on the public NuGet repository with
package name `IntelliFactory.FastInvoke`.

Source is available on
[Bitbucket](http://bitbucket.org/IntelliFactory/fastinvoke).

For Git users there is also a [Github
mirror](http://github.com/intellifactory/fastinvoke).

## Building

Invoke `MSBuild.exe` in the root directory of the checkout.

## Documentation

Use `IntelliFactory.FastInvoke.Compile` to turn a `MethodInfo` into a
`FastMethod`.  Invoking the `FastMethod` with `obj` arguments will be
significantly (several orders of magnitude) faster than invoking the
original method.

## Bugs

Please report bugs and request features using the [Bitbucket
tracker](http://bitbucket.org/IntelliFactory/fastinvoke/issues).

## Contact

This software is being developed by IntelliFactory.  Please feel free
to [contact us](http://websharper.com/contact).

For public discussions we also recommend using
[FPish](http://fpish.net/topics), the functional programming community
site built with [WebSharper](http://websharper.com).
