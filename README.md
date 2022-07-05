![Logo](Logo/Logo.png)

# Bloc interpreter

## Table of content

- [Introduction](#introduction)
- [Interpreter](#interpreter)
- [Example project](#example-project)
- [Language reference](#language-reference)
    - [Values](#values)
    - [Variables](#variables)
    - [Access keywords](#access-keywords)
    - [Operators](#operators)
    - [Statements](#statements)
    - [Commands](#commands)
    - [Comments](#comments)

## Introduction

Bloc is a programming language I originally created for debugging and play testing my games. It started off as just a set of commands I could execute in a console to perform certain actions such as teleporting the player or changing its health. Later, I decided to expand it and make it a complete programming language by adding variables, operators and statements. The language is now separated in two parts, each with a different syntax. The main part has a C-like syntax and is used to handle all the logic. By starting a line with a slash `/`, you can use the second part of the language which consists of a set of commands which can be used to perform actions on objects living outside of Bloc.

## Interpreter

To use the interpreter inside your projects, you have to import the library and create an engine with the Engine.Builder class which is inside the Bloc namespace. You can use this builder to specify where the engine should output text and how to clear the text. You can also give it a set of commands. Once you are done setting up the engine, you can use the Build() method to build it. This method will return an instance of the class Engine. You can then use the Execute() method and pass it a string containing the code to execute. Executing code can return a [Value](#values) if the code was only an expression or a result which can be an Exit or a Throw. If you receive an Exit, you should close the application. If you receive a Throw, it means an exception was thrown and never caught, you should display an error message. For more information, you can look at the example project.

## Example project

The example project is an interactive Bloc console. It allows you to execute a program statement by statement as you write it and allows you to test things quickly to learn the language. If you type a line and press enter, it will execute that line. If you miss a semicolon at the end of that line, this project will add it for you. If the line ends with an opening brace `{`, it will not execute it right away; instead it will let you write as many lines as you want until you close that code block.

## Language reference

### Values

---

#### Void

You can use the `void` literal to create an instance of type void. It cannot be assigned to a variable and is only used when a function returns nothing. However void is implicitly returned when the end of a function is reached or when no value is provided for a return statement, so the `void` literal is rarely used.

```csharp
# The 3 functions return void
var void_1 = () { }; 

var void_2 = () {
    return;
}; 

var void_3 = () {
    return void;
}; 

var foo = void; # throws an exception

typeof void; # evaluates to void
```

---

#### Null

You can use the `null` literal to create an instance of type null. This is used when a variable contains nothing and is the default value of any variable.

```csharp
# Both variables have a value of null
var foo; 

var bar = null; 

typeof null; # evaluates to null
```

---

#### Bool

You can use the `true` and `false` literals to create an instance of type bool. They represent a boolean value and are mainly used as conditions inside `if` statements and loops.

```csharp
var condition = true; 

if (condition)
    /echo 'Hello world'; 

typeof true;    # evaluates to bool
typeof false;   # evaluates to bool
```

---

#### Number

A number is used to represent both integers and floating-point numbers. To create an integer, you can simply write a decimal number with digits from 0 to 9 or you can prefix it with `0b`, `0o` or `0x` to write a number in binary, octal or hexadecimal respectively. To create a floating point number, you can simply add a dot `.` to separate its integral and fractional parts. Floating point numbers have to be decimal, but you can use the scientific notation by adding an `e` and an exponent. Both integers and floating-point literals can contain any number of underscores `_` to separate digits. You can also use the `infinity` and `nan` literals to create a number.

```csharp
# they all represent the same number
42; 
0b_0010_1010; 
0o52; 
0x2A; 

3.141_592_654; 
0.42E-2; 

typeof nan;         # evaluates to number
typeof infinity;    # evaluates to number
typeof -infinity;   # evaluates to number
```

---

#### Range

You can create a range using the [range operator](#range-operator)  `..` and optionally numbers. Ranges can be used to get a slice of a [string](#string) or [array](#array). They can also be used in a [for..in](#forin-statement) loop to iterate over all the integers inside that range. When iterating over a range, if the first number is omitted, it defaults to 0. The second number defaults to `infinity` or `-infinity` depending on the sign of the step. The default step is 1, but you can change it by adding a second range operator `..`. A step of 2 for example will iterate over every second integer inside the range. A negative step will reverse the iteration. For more information on getting a slice of a string or array, see [indexer]().

```csharp
var arr = { 1, 2, 3 }; 
arr[..2];   # evaluates to { 1, 2 }

# outputs the numbers from 0 to 9
for (i in ..10) 
    /echo $i; 

typeof(..); # evaluates to range
```

---

#### String

A string is used to represent text. You can create one by putting a chain of characters between single `'` or double quotes `"`. Whether you use single or double quotes does no change anything at runtime, but using one allows you to create a string containing the other one. A string literal has to be on a single line.

```python
'this is a string'; 
"this is also a string"; 

''  # this is an empty string

'Hello' == "Hello"; # evaluates to true

'this "string" contains double quotes (")'; 
"this 'string' contains single quotes (')"; 
```

If you want a string to include both single and double quotes or simply include special characters, you can use escape sequences with the backslash `\` symbol.

| Escape sequence | Character name  | Unicode encoding |
|-----------------|-----------------|------------------|
| \\'             | Single quote    | 0x0027           |
| \\"             | Double quote    | 0x0022           |
| \\`             | Back quote      | 0x0060           |
| \\\             | Backslash       | 0x005C           |
| \\0             | Null            | 0x0000           |
| \\a             | Alert           | 0x0007           |
| \\b             | Backspace       | 0x0008           |
| \\f             | Form feed       | 0x000C           |
| \\n             | New line        | 0x000A           |
| \\r             | Carriage return | 0x000D           |
| \\t             | Horizontal tab  | 0x0009           |
| \\v             | Vertical tab    | 0x000B           |

You can also create a string with triple quotes (`'''` or `"""`). These types of string are called raw strings and remove the need to use escape sequences. They allow you to create multi-line strings, to include backslashes `\` without escaping them and to include quotes by doubling them.

```python
''' \n '''; # \n is not interpreted as an escape sequence

""" """""" """; # evaluates to ' """ '

'''
This string contains new lines
without using escape sequences.
'''; 
```

You can also create a string with back quotes ``` ` ```. These types of string are called interpolated string. They allow you to embed expressions inside braces `{}` within a string. The content of the braces will be evaluated and implicitly converted to a string before being concatenated to the rest of the string at runtime. This improves readability over concatenating multiple expressions with the [addition operator](#addition-operator) `+`. If you want to actually have curly brackets inside an interpolated string, you have to double them.

```csharp
var foo = 42; 

# both evaluates to 'foo: 42'
nameof foo + ': ' + foo; 
`{nameof foo}: {foo}`; 

`{foo} {{foo}} {{{foo}}}`; # evaluates to '42 {foo} {42}'
```

You can also combine raw strings and interpolated strings with triple back quotes ` ``` `.

````python
var foo = 42; 

``` `` {{ ```; # evaluates to ' ` { '

```
This is the value of foo
inside a multi-line string:
{foo}
```;
````

---

#### Array

An array is used to represent a list of ordered values. You can create one by putting a list of coma `,` separated values inside braces `{}`. The values can be of any type and you are allowed to leave a trailing comma `,` after the last value. You cannot make an empty array literal because it would be ambiguous with an empty [struct](#struct). If you want to create an empty array, use the default constructor for `array`.

```csharp
var myArray = { 1, 2, 3, }; 

var emptyArray = array(); 
```

---

#### Struct

A struct is used to represent a set of key-value pairs. You can create one by putting a list of coma `,` separated key-value pairs  inside braces `{}`. A key-value pair is formed by a name, the equal symbol `=` and a value. The values can be of any type and the keys have to be valid variable names. You are allowed to leave a trailing comma `,` after the last key-value pair. You cannot make an empty struct literal because it would be ambiguous with an empty [array](#array). If you want to create an empty struct, use the default constructor for `struct`.

```csharp
var myStruct = {
    a = 1,
    b = 2,
    c = 3,
}; 

var emptyStruct = struct(); 
```

---

#### Tuple

You can create a tuple by separating two or more expressions with the coma operator `,`. Tuples also are usually surrounded by parentheses `()` because of the [operator precedence](#operators). Tuples are useful to perform an operation on multiple values at the same time. You can perform an operation between all the values of a tuple and a single value or between all the values of two tuples. If the tuples don't have the same size, an exception is thrown.

```csharp
1, 2, 3;        # evaluates to (1, 2, 3)

`{1, 2, 3}`;    # evaluates to '(1, 2, 3)'


(1, 2) + 1;         # evaluates to (2, 3)

(1, 2) + (3, 4);    # evaluates to (4, 6)

(1, 2) + (3, 4, 5); # throws an exception
```

---

#### Function

A function is used to hold statements that you want to execute at more than one place and allows you to reuse code. You can generalize a function by using parameters, allowing you to use that function in even more places. There are two syntax you can use to create a function.

With the first syntax, you have to put a pair of parentheses `()` next to a pair of braces `{}`. The parentheses can contain a list of comma `,` separated identifiers. These are the parameters of the function. The braces can contain a list of statements to execute when the function is called. These statements can access the parameters of the function directly by using the names of the parameters as variables or by using the `params` keyword.

```csharp
var print = (text) {
    /echo $text;
}; 
```

With the second syntax, you have to put a pair of parentheses next to the [lambda operator](#lambda-operator) `=>` next to an expression. The parentheses can be omited if there is exactly one parameter and it does not have an explicit default value. This type of function will evaluate its expression and return the result. This is pretty useful for single line functions.

```csharp
var add = (a, b) => a + b; 

var toString = x => x as string; 
```

Both syntax can be used to create asynchronous function by prefixing them with the `async` keyword. Asynchronous functions allow you to run multiple functions at the same time. When calling an asynchronous function, it instantly returns a [task](#task) and starts executing its body in the background. The task returned can be used to retrieve the result of the function later. If the function throws an exception, the exception will be stored inside the task.

```csharp
var foo = async () {
    var sum = 0;
    
    for (i in ..1000)
        sum += i;

    return sum;
}; 

var bar = async () => await foo(); 
```

---

#### Task

Tasks cannot be directly created, they are returned by [asynchronous functions](#function) and can be used to wait for the function to finish and to retreive the result of the function using the [await operator](#await-operator-await).

```csharp
var myAsyncFunc = async () {
    var sum = 0;

    for (i in ..1000)
        sum += i;

    return sum;
}; 

var myTask = myAsyncFunc(); 
```

---

#### Reference

You can create a reference to a variable by using the `ref` operator on it. You can also use the `new` operator to simultaneously create a variable on the heap and return a reference to it. References are useful if you want multiple variables to point to the same value.

```csharp
var foo = { 1, 2 }; 

var ref1 = ref foo;         # ref1 is a reference to the variable foo

var ref2 = new { 1, 2 };    # ref2 is a reference to an array stored on the heap
```

---

#### Complex

Complexes cannot be directly created. They are returned by some command and are essentially references to objects that live outside of Bloc. You cannot modify them in any way, you can only store them and pass them to another command.

---

#### Type

You can create a type by using any of the following literals : `bool`,  `number`,  `range`,  `string`,  `array`,  `struct`,  `tuple`,  `function`,  `reference`,  `complex`,  `type` and  `any`. You can also use the `typeof` operator to get the type of a value. You have to use the `typeof` operator to get the void or null type since they don't have literals. Types can be used to check if a value is of a certain type with the `is` or `is not` operator. Types can be combined into composit types using bitwise operators or the [nullable type operator](#nullable-type-operator) `?`. This allows you to check if a value is of any of the types within a composite type. `any` is the composite type of every types. Types can also be used to cast a value from its type to another with the `as` operator. A value cannot be cast to a composite type.

```csharp
0 is number;            # evaluates to true
0 is (array | struct);  # evaluates to false

true as number; # evaluates to 1

true as (number | string) # throws an exception
```

---

### Variables

Variables allow you to store values in memory to use later. Values can be stored either on the stack or the heap.

To store a value on the stack, you have to use a `var` statement or the `let` operator. Variables stored on the stack can be accessed by their name but are automatically deleted once you get out of the scope in which they were declared. A variable name can contain both lower and upper-case characters, digits, underscores `_` and any special character except for these : ```!"#$%&'()*+,-./:;<=>?@[\]^`{|}~```. A variable name cannot contain white spaces and cannot start with a digit.

```csharp
# declaring a variable with the let operator
let foo; 

# declaring a variable with a var statement
var bar; 

# assigning it 'Hello world'
bar = 'Hello world'; 

# retrieving its value
bar; 

# you can declare a variable and assign it a value at the same time
var baz = 42; 
```

To store a value on the heap, you have to use the `new` operator. Variables stored on the heap can be accessed from outside their original scope but they do not have names and can only be accessed using [references](#reference). The `new` operator returns a reference to the newly created heap variable. Heap variables are automatically deleted once all their references have been deleted.

```csharp
# creating a string on the heap and
# storing a reference to it in the variable foo
var foo = new 'Hello world'; 

# retrieving its value
val foo; 
```

Some valid variable names can be ambiguous between a variable and a keyword. By default, the interpreter will interpret them as keywords, but you can use the variable identifier character `$` to force the interpreter to interpret them as variables. The `$` **is not** part of the variable name. This means that the `nameof` operator will return the name with out the `$` and when using indexers you have to ommit the `$`. You can use the variable identifier character with any variable even if they are not ambiguous and you have to use it with every variable inside a command.

```csharp
var $for = { 'a', 'b', 'c' };

for (item in $for)
    /echo $item;

nameof $for; # evaluates to 'for'

var foo = {
    $if = true
};

# both point to the same variable
foo.$if;
foo['if'];

var bar = 42;

$bar; # evaluates to 42
```
---

### Access keywords

Most keywords are used as literals or operators or used to initiate a statement, but there are two keywords which can be used as variables. These keywords are `params` and `recall`.

---

#### params

The `params` keyword can be used inside a [function](#function) to access an [array](#array) containing all the parameters that were sent when invoking that function. This is useful if a function has a variable number of arguments. The array is deleted once you exit out of the function.

```csharp
var print = () {
    for (x in params)
        /echo $x;
};
```

---

#### recall

The `recall` keyword can be used inside a [function](#function) to access a copy of that function allowing you to *recall* it. This is the preferred way of doing recursion. You should not try to recursively call a function using its name. The copy is deleted once you exit out of the function.

```csharp
var fibonacci = (n) {
    if (n == 0)
        return 0;

    if (n == 1)
        return 1;

    return recall(n - 1) + recall(n - 2);
}; 
```

---

### Operators

Operators allow you to perform basic operations on values and variables. You can combine values and variables with operators to create complex expressions. An expression can then be evaluated and reduced to a single value. The order in which operators are evaluated is determined by the precedence and associativity of these operators, but you can use parentheses `()` to change that order.

The following table shows the precedence of all operators. The operators at the top of the table are evaluated before the ones at the bottom. If some operators have the same precedence, their associativity will determine the order of operation. 

| Operators                                                                                | Description          | Associativity                     |
|------------------------------------------------------------------------------------------|----------------------|-----------------------------------|
| [`.`](#member-access-operator), [`[]`](#indexer-operator), [`()`](#invocation-operator)  | Primary              | Left&#8209;to&#8209;right&nbsp;🡲 |
| [`+`](#unary-plus-operator), [`-`](#unary-minus-operator-), [`~`](#complement-operator-), [`!`](#negation-operator), [`++`](#increment-operator), [`--`](#decrement-operator-), [`~~`](#variable-complement-operator-), [`!!`](#variable-negation-operator), [`?`](#nullable-type-operator), [`len`](#length-operator-len), [`chr`](#character-operator-chr), [`ord`](#ordinal-operator-ord), [`val`](#value-operator-val), [`ref`](#reference-operator-ref), [`let`](#definition-operator-let), [`new`](#allocation-operator-new), [`delete`](#delete-operator-delete), [`await`](#await-operator-await) [`nameof`](#nameof-operator-nameof), [`typeof`](#typeof-operator-typeof) | Unary | Right&#8209;to&#8209;left&nbsp;🡰 |
| [`..`](#range-operator)                                                                  | Range                | N/A                               |
| [`**`](#power-operator), [`//`](#root-operator), [`%%`](#logarithm-operator)             | Exponential          | Left&#8209;to&#8209;right&nbsp;🡲 |
| [`*`](#multiplication-operator), [`/`](#division-operator), [`%`](#remainder-operator)   | Multiplicative       | Left&#8209;to&#8209;right&nbsp;🡲 |
| [`+`](#addition-operator), [`-`](#subtraction-operator-)                                 | Additive             | Left&#8209;to&#8209;right&nbsp;🡲 |
| [`<<`](#left-shift-operator), [`>>`](#right-shift-operator)                              | Shift                | Left&#8209;to&#8209;right&nbsp;🡲 |
| [`<=>`](#three-way-comparison-operator)                                                  | Three-way comparison | Left&#8209;to&#8209;right&nbsp;🡲 |
| [`<`](#less-than-operator), [`>`](#greater-than-operator), [`<=`](#less-than-or-equal-operator), [`>=`](#greater-than-or-equal-operator), [`in`](#in-operator-in), [`not in`](#not-in-operator-not-in), [`is`](#is-operator-is), [`is not`](#is-not-operator-is-not), [`as`](#as-operator-as) | Relation | Left&#8209;to&#8209;right&nbsp;🡲 |
| [`==`](#equality-operator), [`!=`](#inequality-operators), [`<>`](#inequality-operators) | Equality             | Left&#8209;to&#8209;right&nbsp;🡲 |
| [`&`](#bitwise-and-operator)                                                             | Bitwise AND          | Left&#8209;to&#8209;right&nbsp;🡲 |
| [`^`](#bitwise-xor-operator)                                                             | Bitwise XOR          | Left&#8209;to&#8209;right&nbsp;🡲 |
| [`\|`](#bitwise-or-operator)                                                             | Bitwise OR           | Left&#8209;to&#8209;right&nbsp;🡲 |
| [`&&`](#boolean-and-operator)                                                            | Boolean AND          | Left&#8209;to&#8209;right&nbsp;🡲 |
| [`^^`](#boolean-xor-operator)                                                            | Boolean XOR          | Left&#8209;to&#8209;right&nbsp;🡲 |
| [`\|\|`](#boolean-or-operator)                                                           | Boolean OR           | Left&#8209;to&#8209;right&nbsp;🡲 |
| [`?:`](#ternary-conditional-operator)                                                    | Ternary              | Right&#8209;to&#8209;left&nbsp;🡰 |
| [`=`](#assignment-operator), [`+=`, `-=`, `*=`, `/=`, `%=`, `**=`, `//=`, `%%=`, `<<=`, `>>=`, `&=`, `\|=`, `^=`, `&&=`, `\|\|=`, `^^=`](#compound-assignment-operators) | Assignment | Right&#8209;to&#8209;left&nbsp;🡰 |
| [`=>`](#lambda-operator)                                                                 | Lambda               | Right&#8209;to&#8209;left&nbsp;🡰 |
| [`,`](#comma-operator)                                                                   | Comma                | N/A                               |

The pipe operator `|>` is not in that table because it can only be used inside a command statement which cannot have any other operators. Its associativity is from left to right.

Unrelated to operator precedence and associativity, an operator will evaluate its operands from left to right.

Note that the operands don't need to be exactly the type specified in a certain context, they only need to be [implicitly convertible]() to that type. In the case where the operands are implicitly convertible to multiple types and match more than one operator overload, the upper most one takes priority. In the case where the operands do not match any overload, an exception is thrown.

---

#### Addition operator `+`

If both its operands are [numbers](#number), it will compute the sum of its operands.

```csharp
5 + 4.6; # evaluates to 9.6
```

If both its operands are [arrays](#array), it will concatenate them.

```csharp
({ 4, 4 } + { 2, 7 }); # evaluates to { 4, 4, 2, 7 }
```

If its left operand is an [array](#array), but the right one is not, it will append the right operand to the [array](#array).

```csharp
({ 2, 5 } + 2); # evaluates to { 2, 5, 2 }
```

If its right operand is an [array](#array), but the left one is not, it will prepend the left operand to the [array](#array).

```csharp
2 + { 3, 8 }; # evaluates to { 2, 3, 8 }
```

If both its operands are [strings](#string), it will concatenate them.

```csharp
'fizz' + 'buzz'; # evaluates to 'fizzbuzz'
```

Note that [string interpolation]() provides a more convenient way to format strings.

---

#### Subtraction operator `-`

If both its operands are [numbers](#number), it will subtract its right operand from its left operand.

```csharp
5 - 3;      # evaluates to 2
7 - 5.8;    # evaluates to 1.2
5 - 9;      # evaluates to -4
```

---

#### Multiplication operator `*`

If both its operands are [numbers](#number), it will compute the product of its operands.

```csharp
2 * 8.6; # evaluates to 17.2
```

If one of its operands is an [array](#array) and the other is a [number](#number), it will concatenate an empty array with the given array a number of times equivalent to the floor of the given number.

```csharp
({ 1, 2 } * 3);     # evaluates to { 1, 2, 1, 2, 1, 2 }
({ 1, 2 } * 0);     # evaluates to array()
({ 1, 2 } * 1.9);   # evaluates to { 1, 2 }
({ 1, 2 } * -2);    # throws an exception
```

If one of its operands is a [string](#string) and the other is a [number](#number), it will concatenate an empty string with the given string a number of times equivalent to the floor of the given number.

```csharp
'ab' * 3;   # evaluates to 'ababab'
'ab' * 0;   # evaluates to ''
'ab' * 1.9; # evaluates to 'ab'
'ab' * -2;  # throws an exception
```

---

#### Division operator `/`

If both its operands are [numbers](#number), it will divide its left operand by its right operand. Dividing a positive number by zero will return `infinity`, dividing a negative number by zero will return `-infinity` and dividing zero by zero will return `nan`.

```csharp
6 / 4;  # evaluates to 1.5
1 / 0;  # evaluates to infinity
-1 / 0; # evaluates to -infinity
0 / 0;  # evaluates to nan
```

The `/` can also be used to make a [command statement]().

---

#### Remainder operator `%`

If both its operands are [numbers](#number), it will compute the remainder after dividing its left operand by its right operand. Even though it is similar, this **is not** a modulus operator. It can be used as a modulus operator as long as its operands are positive, but with negative values the result will not be the same.

```csharp
8 % 3;      # evaluates to 2
8.4 % 1;    # evaluates to 0.4
```

---

#### Power operator `**`

If both its operands are [numbers](#number), it will raise its left operand to the power of its right operand.

```csharp
5 ** 4;     # evaluates to 625
10 ** -1;   # evaluates to 0.1
25 ** 0.5;  # evaluates to 5
```

---

#### Root operator `//`

If both its operands are [numbers](#number), it will compute the n^th^ root of its left operand, where n is the value of its right operand.

```csharp
25 // 2; # evaluates to 5 (the square root of 25)
```

---

#### Logarithm operator `%%`

If both its operands are [numbers](#number), it will compute the logarithm of its left operand with its right operand as the base.

```csharp
100 %% 10; # evaluates to 2 (the logarithm base 10 of 100)
```

---

#### Unary plus operator `+`

If its operand is a [number](#number), it will return the value of that number. It can be used to [implicitly convert]() a value to a [number](#number).

```csharp
+4;     # evaluates to 4
+true;  # evaluates to 1
```

---

#### Unary minus operator `-`

If its operand is a [number](#number), it will compute the numeric negation of its operand.

```csharp
-4;     # evaluates to -4
- -4;   # evaluates to 4
```

Note that there is a space between the two `-`. If we remove that space, they will be interpreted as the [decrement operator `--`](#decrement-operator-). 

---

#### Increment operator `++`

If its operand has a [number](#number) as its value, it will increment it by 1. The operand must be a variable. This operator can be used both as a prefix and a suffix. When used as a prefix, it returns the value of the variable *after* the increment. When used as a suffix, it returns the value of the variable *before* the increment, but after the implicit conversion.

```csharp
var foo = 0; 
++foo;          # evaluates to 1 and foo has a value of 1

var bar = 0; 
bar++;          # evaluates to 0, but bar has a value of 1

0++;            # throws an exception
```

---

#### Decrement operator `--`

If its operand has a [number](#number) as its value, it will decrement it by 1. The operand must be a variable. This operator can be used both as a prefix and a suffix. When used as a prefix, it returns the value of the variable *after* the decrement. When used as a suffix, it returns the value of the variable *before* the decrement, but after the implicit conversion.

```csharp
var foo = 0; 
--foo;          # evaluates to -1 and foo has a value of -1

var bar = 0; 
bar--;          # evaluates to 0, but bar has a value of -1

0--;            # throws an exception
```

---

#### Boolean AND operator `&&`

This operator will compute the logical AND of its operands. Its operands are implicitly converted to bools for the evaluation, but the result will not necessarily be of type `bool`. The operator returns its left operand if it evaluates to `false` otherwise it returns its right operand. When using bools, the result is `true` if both its operands evaluate `true`. Otherwise the result is `false`. If its left operand evaluates to `false`, its right operand is not evaluated.

```csharp
var result;

var operand = (value, text) {
    /echo $text;
    return value;
}; 

result = operand(false, 'left evaluated') && operand(true, 'right evaluated'); 
/echo $result; 
# output:
# left evaluated
# false

result = operand(true, 'left evaluated') && operand(true, 'right evaluated'); 
/echo $result; 
# output:
# left evaluated
# right evaluated
# true
```

---

#### Boolean OR operator `||`

This operator will compute the logical OR of its operands. Its operands are implicitly converted to bools for the evaluation, but the result will not necessarily be of type `bool`. The operator returns its left operand if it evaluates to `true` otherwise it returns its right operand. When using bools, the result is `true` if one of its operands evaluates `true`. The result is `false` if both its operands evaluate `false`. If its left operand evaluates to `true`, its right operand is not evaluated.

```csharp
var result;

var operand = (value, text) {
    /echo $text;
    return value;
}; 

result = operand(false, 'left evaluated') || operand(true, 'right evaluated'); 
/echo $result; 
# output:
# left evaluated
# right evaluated
# true

result = operand(true, 'left evaluated') || operand(true, 'right evaluated'); 
/echo $result; 
# output:
# left evaluated
# true

var foo = null; 
var bar = {
    a = 2
}; 

result = foo || bar; 
/echo $result; 
# output:
# {
#     a = 2
# }
```

---

#### Boolean XOR operator `^^`

This operator will compute the logical XOR of its operands. Its operands are implicitly converted to bools for the evaluation, but the result will not necessarily be of type `bool`. If one of its operands evaluates `true` and the other evaluates to `false`, the operator returns the one which evaluated to true. If both its operands evaluate `true` or if both its operands evaluate `false`, the operator returns `null` which is implicitly convertible to `false`. Both operands are always evaluated.

```csharp
var foo, bar = { a = 2 }; 

var result = foo ^^ bar; 
/echo $result; 
# output:
# {
#     a = 2
# }

foo = { b = 5 }; 
bar = { a = 2 }; 

result = foo ^^ bar; 
/echo $result; 
# output:
# null
```

---

#### Negation operator `!`

This operator will compute the logical negation of its operand.The result is `true`, if its operand evaluates to `false`, and `false`, if its operand evaluates to `true`.

```csharp
!true;  # evaluates to false
!false; # evaluates to true
```

---

#### Variable negation operator `!!`

This operator will compute the logical negation of its operand and assign it the result. The operand must be a variable. This operator can be used both as a prefix and a sufix. When used as a prefix, it returns the value of the variable *after* the negation. When used as a sufix, it returns the value of the variable *before* the negation, but after the implicit conversion.

```csharp
var foo = false; 
!!foo;          # evaluates to true and foo has a value of true

var bar = false; 
bar!!;          # evaluates to false, but bar has a value of true

var num = 1; 
num!!;          # evaluates to true, but num has a value of false

false!!;        # throws an exception
```

---

#### Bitwise AND operator `&`

If both its operands are [numbers](#number), it will compute the bitwise logical AND of its operands.

```csharp
var foo = 0b0011; 
var bar = 0b1010; 

foo & bar; # evaluates to 0b0010
```

If both its operands are [types](#type), it will return a composite type whose types are in both operands

```csharp
var foo = array | struct; 
var bar = array | tuple; 

foo & bar; # evaluates to array
```

---

#### Bitwise OR operator `|`

If both its operands are [numbers](#number), it will compute the bitwise logical OR of its operands.

```csharp
var foo = 0b0011; 
var bar = 0b1010; 

foo | bar; # evaluates to 0b1011
```

If both its operands are [types](#type), it will return a composite type whose types are in either operand.

```csharp
var foo = array | struct; 
var bar = array | tuple; 

foo | bar; # evaluates to array | struct | tuple
```

---

#### Bitwise XOR operator `^`

If both its operands are [numbers](#number), it will compute the bitwise logical XOR of its operands.

```csharp
var foo = 0b0011; 
var bar = 0b1010; 

foo ^ bar; # evaluates to 0b1001
```

If both its operands are [types](#type), it will return a composite type whose types are in exactly one of its operands.

```csharp
var foo = array | struct; 
var bar = array | tuple; 

foo ^ bar; # evaluates to struct | tuple
```

---

#### Left-shift operator `<<`

If both its operands are [numbers](#number), it will shift to the left its left operand by the number of bits defined by its right operand.

```csharp
var foo = 0b_1111_0000_0000_0000_0000_0000_0000_1111; 

foo << 4; # evaluates to 0b_0000_0000_0000_0000_0000_0000_1111_0000
```

---

#### Right-shift operator `>>`

If both its operands are [numbers](#number), it will shift to the right its left operand by the number of bits defined by its right operand.

```csharp
var foo = 0b_1111_0000_0000_0000_0000_0000_0000_1111; 

foo >> 4; # evaluates to 0b_1111_1111_0000_0000_0000_0000_0000_0000
```

---

#### Complement operator `~`

If both its operands are [numbers](#number), it will compute the bitwise complement of its operand by reversing each bit.

```csharp
var foo = 0b_1111_0000_0000_1111_1111_0000_0000_1111; 

~foo; # evaluates to 0b_0000_1111_1111_0000_0000_1111_1111_0000
```

If both its operands are [types](#type), it will return a composite type whose types are all types that were not in its operand.

```csharp
var foo = string | array | struct | tuple | function | reference | complex; 

~foo; # evaluates to (void | null | bool | number | range | type)
```

---

#### Variable complement operator `~~`

If its operand has a [number](#number) as its value, it will compute its bitwise complement and assign it the result. The operand must be a variable. This operator can be used both as a prefix and a sufix. When used as a prefix, it returns the value of the variable *after* the complement is assigned. When used as a sufix, it returns the value of the variable *before* the complement, but after the implicit conversion.

```csharp
var foo = 0b_1111_0000_0000_1111_1111_0000_0000_1111;  
~~foo;
# evaluates to 0b_0000_1111_1111_0000_0000_1111_1111_0000 and
# foo has a value of 0b_0000_1111_1111_0000_0000_1111_1111_0000

var bar = 0b_1111_0000_0000_1111_1111_0000_0000_1111; 
bar~~;
# evaluates to 0b_1111_0000_0000_1111_1111_0000_0000_1111 but,
# foo has a value of 0b_0000_1111_1111_0000_0000_1111_1111_0000

0~~; # throws an exception
```

If its operand has a [type](#type) as its value, it will return a composite type whose types are all types that were not in its operand and assign it the result. The operand must be a variable. This operator can be used both as a prefix and a sufix. When used as a prefix, it returns the value of the variable *after* the complement is assigned. When used as a sufix, it returns the value of the variable *before* the complement, but after the implicit conversion.

```csharp
var foo = string | array | struct | tuple | function | reference | complex;  
~~foo;
# evaluates to (void | null | bool | number | range | type) and
# foo has a value of (void | null | bool | number | range | type)

var bar = string | array | struct | tuple | function | reference | complex; 
bar~~;
# evaluates to (string | array | struct | tuple | function | reference | complex) but,
# foo has a value of (void | null | bool | number | range | type)

number~~; # throws an exception
```

---

#### Nullable type operator `?`

If its operand is a [type](#type), it will return a composite type whose types are the ones from its operand plus the null type.

```csharp
# Both evaluates to the same composite type
number?; 
number | typeof null; 
```

---

#### Equality operator `==`

This operator returns `true` if its operands are equal and returns `false` otherwise. If the operands don't have the same type, the result is `false`. If both operands are reference, it returns `true` if they both point to the same variable. If they point to different variables, the result is `false` even if both variables have the same value.

```csharp
"Hello" == "Hello"; # evaluates to true
"Hello" == "World"; # evaluates to false

1 == '1'; # evaluates to false

var foo = new { a = 2 }; 
var bar = new { a = 2 }; 

foo == bar;         # evaluates to false
val foo == val bar; # evaluates to true
```

---

#### Inequality operators `!=`, `<>`

These operators both return `true` if their operands are **not** equal and return `false` otherwise. It does the opposite of the [equality operator](#equality-operator). Both inequality operators can be used, but you should only use one of them per project.

```csharp
# they all evaluates to true
1 <> "1"; 
1 != "1"; 
!(1 == "1"); 
```

---

#### Less than operator `<`

If both its operands are [numbers](#number), it will return `true` if its left operand is less than its right operand and return `false` otherwise.

```csharp
2 < 1; # evaluates to false
2 < 2; # evaluates to false
2 < 5; # evaluates to true
```

---

#### Greater than operator `>`

If both its operands are [numbers](#number), it will return `true` if its left operand is greater than its right operand and return `false` otherwise.

```csharp
2 > 1; # evaluates to true
2 > 2; # evaluates to false
2 > 5; # evaluates to false
```

---

#### Less than or equal operator `<=`

If both its operands are [numbers](#number), it will return `true` if its left operand is less than or equal to its right operand and return `false` otherwise.

```csharp
2 <= 1; # evaluates to false
2 <= 2; # evaluates to true
2 <= 5; # evaluates to true
```

---

#### Greater than or equal operator `>=`

If both its operands are [numbers](#number), it will return `true` if its left operand is greater than or equal to its right operand and return `false` otherwise.

```csharp
2 >= 1; # evaluates to true
2 >= 2; # evaluates to true
2 >= 5; # evaluates to false
```

---

#### Three-way comparison operator `<=>`

If both its operands are [numbers](#number), it will return `1` if its left operand is greater than its right operand, return `-1` if its left operand is less than its right operand and return `0` if both operands are equal.

```csharp
2 <=> 1; # evaluates to 1
2 <=> 2; # evaluates to 0
2 <=> 5; # evaluates to -1
```

---

#### Assignment operator `=`

This operator assigns the value of its right operand to a variable or an element of an array or struct given by its left operand. It also returns the value assigned to the left operand.

```csharp
var a, b; 

a = 2;      # assigns 2 to the variable a

a = b = 5;  # assigns 5 to b and then to a
```

---

#### Compound assignment operators

Compound assignment operators are formed by combining some binary operators and the assignment operator. Here are all compound assignment operators :

`+=`, `-=`, `*=`, `/=`, `%=`, `**=`, `//=`, `%%=`, `<<=`, `>>=`, `&=`, `|=`, `^=`, `&&=`, `||=`, `^^=`

Given a binary operator `op`, the expression `x op= y` is equivalent to `x = x op y`.

`x += 1` is equivalent to `++x` and `x -= 1` is equivalent to `--x`.

---

#### Member access operator `.`

This operator is used to access a member of a struct. If the member does not exist, an exception is thrown.

```csharp
var foo = {
    a = 2,
    b = {
        c = 5
    }
}; 

foo.a;      # evaluates to 2
foo.b.c;    # evaluates to 5

foo.e;      # throws an exception
```

---

#### Indexer operator `[]`

This operator can be used to index elements of a string, an array or a struct. Inside the brackets, you have to provide exactly 1 parameter.

If used on a [string](#string) with a [number](#number) as the parameter, it returns a string containing the single character from the original string at the index given by the parameter. Negative indices can be used to access characters from the end of the string. If the index is outside the bounds of the string, an exception is thrown.

```csharp
var foo = 'Hello'; 

foo[0];     # evaluates to 'H'
foo[1];     # evaluates to 'e'
foo[-1];    # evaluates to 'o'

foo[5];     # throws an exception
foo[-6];    # throws an exception
```

If used on a [string](#string) with a [range](#range) as the parameter, it returns a string containing a slice of the original string. If the slice goes outside the bounds of the string, an exception is thrown.

```csharp
var foo = 'Hello'; 

foo[1..-1]; # evaluates to 'ell'
```

If used on an [array](#array) with a [number](#number) as the parameter, it returns the element inside the array at the index given by the parameter. Negative indices can be used to access elements from the end of the array. If the index is outside the bounds of the array, an exception is thrown.

```csharp
var foo = { 'a', 'b', 'c', 'd', 'e' }; 

foo[0];     # evaluates to 'a'
foo[1];     # evaluates to 'b'
foo[-1];    # evaluates to 'e'

foo[5];     # throws an exception
foo[-6];    # throws an exception

var bar = {
    { 1, 2, 3 },
    { 4, 5, 6 },
    { 7, 8, 9 }
}; 

bar[0][1]; # evaluates to 2
```

If used on an [array](#array) with a [range](#range) as the parameter, it returns an array containing a slice of the original array. If the slice goes outside the bounds of the array, an exception is thrown.

```csharp
var foo = { 'a', 'b', 'c', 'd', 'e' }; 

foo[1..-1]; # evaluates to { 'b', 'c', 'd' }
```

If used on an [array](#array) with a [function](#function) as the parameter, for each element in the array, it calls the function and passes it the element as the only parameter and returns a array where each element is the result of the each of the function calls.

```csharp
var foo = { 1, 2, 3, 4, 5 }; 

foo[x => x + 1]; # evaluates to { 2, 3, 4, 5, 6 }
```

If used on a [struct](#struct) with a [string](#string) as the parameter, it returns the member of the struct whose name is equal to the parameter. The string has to be a valid variable name. If the member does not exist, it is created.

```csharp
var foo = {
    a = 2
}; 

foo['a']; # evaluates to 2
```

Note that the preferred way to access a member of a struct is to use the [member access operator](#member-access-operator). You should only use an indexer to access a member whose name is determined at runtime.

---

#### Invocation operator `()`

This operator can be used both to call a [function](#function) or to call the constructor of a [type](#type). You can provide parameters for the function or the constructor by putting a list of comma-separated values inside the parentheses.

```csharp
var add = (a, b) => a + b; 

add(1, 2);  # calls the function in the variable add with 1 and 2 as parameters

array();    # calls the array constructor without parameters
```

---

#### Range operator `..`

This operator is used to create [ranges](#range) which can be used to get a slice of a string or array or to iterate over all integers inside the range. The start of the range is given by the left operand and the end of the range is given by the right operand. The start of the range is inclusive, but the end is exclusive. Negative indices can be used to reference indices from the end of a string or array. You can add a second range operator after the right operand and add a third operand to specify the step of the range. Creating a range with a step of 0 will throw an exception.

```csharp
var foo = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }; 

foo[1..4];      # evaluates to { 1, 2, 3 }
foo[-4..-1];    # evaluates to { 6, 7, 8 }
foo[3..-3];     # evaluates to { 3, 4, 5, 6 }
foo[1..8..2];   # evaluates to { 1, 3, 5, 7 }
foo[4..0..-1];  # evaluates to { 4, 3, 2, 1 }
foo[-2..1..-2]; # evaluates to { 8, 6, 4, 2 }
foo[1..8..0];   # throws an exception
```

All operands can be omitted to use their default value. The step always defaults to 1, but the start and end will default to different values depending on the context.

When used with the indexer operator, the start defaults to 0 if the step is positive or the last index if the step is negative. The end default to the size of the string or array if the step is positive or -1 if the step is negative.

```csharp
var foo = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }; 

foo[..5];       # evaluates to { 0, 1, 2, 3, 4 }
foo[..5..-1];   # evaluates to { 9, 8, 7, 6 }
foo[5..];       # evaluates to { 5, 6, 7, 8, 9 }
foo[5....-1];   # evaluates to { 5, 4, 3, 2, 1, 0 }

var bar = { 0, 1, 2, 3, 4 }; 

bar[..];        # evaluates to { 0, 1, 2, 3, 4 }
bar[....-1];    # evaluates to { 4, 3, 2, 1, 0 }
```

When used inside a for..in loop, the start defaults to 0 if the step is positive or -1 if the step is negative. The end defaults to infinity if the step is positive or -infinity if the step is negative.

```csharp
for (i in ..5)
    /echo $i;
# output
# 0
# 1
# 2
# 3
# 4

for (i in ..-5..-1)
    /echo $i;
# output
# -1
# -2
# -3
# -4
```

---

#### Ternary conditional operator `?:`

This operator has three operands : the condition, the consequent and the alternative. This is its syntax : `condition ? consequent : alternative`. The condition is always evaluated and must return a [bool](#bool). If the condition evaluates to `true`, the consequent is evaluated and its result is returned. If the condition evaluates to `false`, the alternative is evaluated and its result is returned. The consequent and the alternative cannot be both evaluated. If the condition cannot be implicitly converted to a [bool](#bool), an exception is thrown and none of the other two expressions are evaluated. This operator can be used as an inline [if statement](#if-statement).

```csharp
var print = (text) {
    /echo $text;
}; 

var temperature; 

temperature = 15; 
print(temperature < 25 ? 'cold' : 'hot'); # outputs cold

temperature = 30; 
print(temperature < 25 ? 'cold' : 'hot'); # outputs hot

print("") ? true : false; # throws an exception since print returns void
```

---

#### Lambda operator `=>`

This operator is used to create single line functions. On the left side of the operator, you can define the parameters of the function. If the function has exactly one parameter and this operator does not have an explicit default value, you can simply write its name. Otherwise, the parameters have to be between parentheses and separated by commas. On the right side of the operator, you have to give it an expression. The resulting function will evaluate this expression and return its result.

```csharp
var toString = x => x as string; 

var add = (a, b) => a + b; 

var foo = { 0, 1, 2, 3, 4 }; 

foo[x => x + 1]; # evaluates to { 1, 2, 3, 4, 5 }
```

---

#### Comma operator `,`

Commas can be used to separate the values of an array, the values of a struct, the parameters of a function declaration or the parameters sent with the invocation operator. Outside of these contexts, a comma will be interpreted as the comma operator. This operator is used to create [tuples](#tuple).

```csharp
1, 2, 3; # evaluates to (1, 2, 3)

var foo = (1, 2, 3); 

var (a, b, c) = foo; 

a; # evaluates to 1
b; # evaluates to 2
c; # evaluates to 3
```

---

#### In operator `in`

If its right operand is an [array](#array), it will return `true` if the array contains the value of its left operand, `false` otherwise.

```csharp
2 in { 1, 2, 3 }; # evaluates to true

2 in { 4, 5, 6 }; # evaluates to false
```

If both its operands are [strings](#string), it will return `true` the right string contains the left one, `false` otherwise. The search is case sensitive.

```csharp
"Hello" in "Hello world";   # evaluates to true

"Asdf" in "Hello world";    # evaluates to false

"hello" in "Hello world";   # evaluates to false
```

---

#### Not in operator `not in`

This operator does the opposite of the [in operator](#in-operator-in).

```csharp
# both evaluate to true
2 not in { 4, 5, 6 }; 
!(2 in { 4, 5, 6 }); 

# both evaluate to true
"Asdf" not in "Hello world"; 
!("Asdf" in "Hello world"); 
```

---

#### Is operator `is`

This operator returns `true` if the type of the left operand is equal to the type given by its right operand and returns `false` otherwise. Its right operand has to be a [type](#type).

```csharp
2 is number; # evaluate to true

2 is string; # evaluates to false

type is type; # evaluates to true
```

This operator can also be used with composite types. In which case, it returns `true` if the composite type contains the type of the left operand, `false` otherwise.

```csharp
2 is (bool | number); # evaluate to true

2 is (bool | string); # evaluates to false

# this produces the same result without using the is operator
(typeof 2 & (bool | string)) != ~any; 
```

---

#### Is not operator `is not`

This operator does the opposite of the [is operator](#is-operator-is).

```csharp
# both evaluate to true
2 is not string; 
!(2 is string); 
```

---

#### As operator `as`

This operator performs an explicit conversion of the result of it left operand to the type given by its right operand. If its right operand is not a [type](#type), if the type is composite or if a conversion to that type does not exist, an exception is thrown.

```csharp
1 as bool; # evaluate to true

1 as string; # evaluate to '1'

1 as (bool | string); # throws an exception

1 as array; # throws an exception
```

---

#### Length operator `len`

If its operand is an [array](#array), it returns the number of elements inside that array.

```csharp
len array();            # evaluates to 0
len { 0, 1, 2, 3, 4 };  # evaluates to 5
```

If its operand is a [string](#string), its return the number of characters in that string.

```csharp
len '';         # evaluates to 0
len 'Hello';    # evaluates to 5
```

---

#### Character operator `chr`

This operator returns a [string](#string) containing a single character whose character code is equal to the operand of the operator. Its operand has to be a [number](#number).

```csharp
chr 0;  # evaluates to '\0'
chr 65; # evaluates to 'A'
```

---

#### Ordinal operator `ord`

This operator returns a [number](#string) equal to the character code of the single character of its operand. If its operand is not a [string](#string) or contains more than 1 character, an exception is thrown.

```csharp
ord '\0';   # evaluates to 0
ord 'A';    # evaluates to 65

ord 'AB'; # throws an exception
```

---

#### Value operator `val`

This operator will dereference every reference in a chain of references until it finds a value and return this value. The result of this operator is guaranteed not to be a reference. If its operand is not a reference, its value is simply returned. If too many references are dereferenced, an exception is thrown. This is the hop limit and prevents the program from freezing if two references are pointing at each other creating an infinite loop.

```csharp
val 2; # evaluates to 2

var value = 5; 
var ref1 = ref value; 
var ref2 = ref ref1; 

val value;  # evaluates to 5
val ref1;   # evaluates to 5
val ref2;   # evaluates to 5

var foo; 
var bar = ref foo; 
foo = ref bar; 

val foo; # throws an exception
```

---

#### Reference operator `ref`

This operator returns a reference to its operand. Its operand has to be a variable.

```csharp
var foo = 2; 
var bar = ref foo; # bar is a reference

val bar;        # evaluates to 2
nameof val bar; # evaluates to 'foo'
```

---

#### Definition operator `let`

This operator allows you to define variables. Its operand can be an identifier or a tuple of identifiers and it will return the declared variables. It is similar to a var statement, but it can be used inside expressions with some restrictions. Firstly, you cannot use initial assignments. A similar behaviour can be achieved by combining it with an assignment operator, but the variable will be defined before the right expression gets evaluated. Secondly, you cannot do multiple definitions with a single `let` operator, you have to use a tuple, or multiple `let` operators. This operator is mainly used inside for loops and with function returning values from their parameters.

```csharp
let foo; # defines a variable named foo

var bar = ref bar; # throws an exception
let bar = ref bar; # works fine

# Declaring multiple variable with a var statement
var a = 2, b = 5; 

# Two ways to acheive the same thing with the let operator
let (c, d) = (2, 5); 
let e = 2, let f = 5; 

# Using let inside a for loop
for (let i = 0; i < 10; i++)
    /echo $i;

# Another example
var tryParseNumber = (input, output) {
    try {
        val output = input as number; 
        return true;
    } catch {
        val output = null;
        return false;
    }
}; 

if (tryParseNumber(5, ref let num))
    /echo `The number was {num}`;
```

---

#### Allocation operator `new`

This operator creates a copy of its operand on the heap and returns a reference pointing to that copy. For more information on the heap, see [Variables](#variables).

```csharp
var foo = new {
    a = 2
}; 
# foo is a reference to a struct stored on the heap
```

---

#### Delete operator `delete`

The delete operator allows you to delete variable previously declared. Its operand can be a variable or a tuple of variables and it will return the value stored in the deleted variables. You can also delete heap variables by combining it with the [val operator](#value-operator-val). You cannot delete elements of an array or struct.

```csharp
var foo; 
delete foo; # The variable foo no longer exists

var bar = new { 1, 2, 3 }; 
delete val bar; # The variable bar contains an invalid reference

var a, b, c; 
delete (a, b, c); 
```

---

#### Await operator `await`

This operator waits for the end of the operation associated with a task to finish and then returns its result. If the operation threw an exception, the await operator will throw that same exception.

```csharp
var addAsync = async (a, b) {
    if (a is not number || b is not number)
        throw "The parameters must be numbers";

    return a + b;
}; 

var task1 = addAsync(1, 2); 

await task1(); # evaluates to 3

var task2 = addAsync("", ""); # no exception is thrown 

await task2();  # throws the exception
```

---

#### Nameof operator `nameof`

This operator returns a [string](#string) containing the name of its operand. Its operand has to be a variable. This operator is especially useful in combination with the [value operator](#value-operator-val) to know the variable at which a reference is pointing.

```csharp
var foo, bar = ref foo; 

nameof foo;     # 'foo'
nameof bar;     # 'bar'
nameof val bar; # 'foo'
```

---

#### Typeof operator `typeof`

This operator returns the [type](#type) of its operand.

```csharp
typeof 2;                   # evaluates to number
typeof 'Hello';             # evaluates to string
typeof { 0, 1, 2, 3 ,4 };   # evaluates to array
```

---

#### Pipe operator `|>`

By default, the output of a command is displayed on the standard output. This operator allows you to redirect the output of a command to the input of another command. The redirected output is not displayed on the standard output.

```csharp

```

---

### Statements

Statements represent the actions that the program will perform. By default, statements are executed from top to bottom, but some statements allow you to change the order in which they get executed or whether or not they get executed at all. Most statements start with a keyword and end with a semicolon `;`. Here is an example of a [var statement](#var-statement):

```csharp
var foo = 'Hello world'; 
```

Statements can also be grouped together in a statement block using braces `{}`. [Stack variables](#variables) can only be accessed within the block they were declared in.

```csharp
{
    var foo = 'Hello world'; 
    /echo $foo; 
}

/echo $foo; # throws an exception
```

Some statements require an embedded statement. This statement can either be a single statement or a statement block.

```csharp
if (true) {
    var foo = 'Hello world'; 
    /echo $foo; 
}

if (true)
    /echo 'Hello world'; 

```

You can also label statements by prefixing them with a label and the label identifier `::`. The label has to be a valid [variable name](#variables). A label and a variable can have the same name and won't interfere with each other. Labels can be used with [goto statements](#goto-statement) to jump to a certain statement and start executing from there.

```csharp
Loop :: /echo 'Hello world'; 
goto Loop; 
```

---

#### Expression statement

The expression statement is the most common type of statement. It only consists of an expression followed by a semicolon `;`. This type of statement is useful to assign a value to a variable or to invoke a function.

```csharp
var print; 

# Expression statement assigning a function to a variable
print = (text) {
    /echo &text;
}; 

# Expression statement invoking a function
print('Hello world'); 
```

---

#### Command statement

Command statements allow you to run commands. It consists of a slash `/` followed by one or more commands and ends with a semicolon `;`. Each command of a command statement must be separated by the [pipe operator](#pipe-operator).

```csharp
/random 10 |> set value; 
```

---

#### Empty statement

The empty statement only consists of a semicolon `;` and does nothing. It can be useful where a statement is required but you don't need one in your use case.

```csharp
while (proccessData()); 
```

---

#### pass statement

The pass statement consists of the `pass` keyword followed by a semicolon `;`. It acts the same as an [empty statement](#empty-statement), but is more explicit.

```csharp
while (proccessData())
    pass; 
```

---

#### var statement

The var statement allows you to declare [variables](#variables). It consists of the `var` keyword followed by a list of comma `,` separated declarations and ends with a semicolon `;`. There must be at leat 1 declaration in a var statement. A declaration starts with a single identifier or a [tuple](#tuple) of identifiers. You can also nest tuples. Optionally, you can add an initial assignment by adding an equal symbol `=` and an expression on the right which will be assigned to the left part of the declaration. When assigning a tuple to a tuple of identifiers, both tuple must have the same size, otherwise an exception is thrown. If an initial assignment is provided, the expression is evaluated first, then the variable gets declared and finally it gets assigned the value. Declarations are evaluated from left to right.

```csharp
# Simple declaration
var a; 

# Declaration with initial assignement
var b = 2; 

# Multiple declarations
var c = 3, d = 4, e = 5; 

# Single declaration using tuples
var (f, g, h) = (6, 7, 8); 

# Complex declarations (you probably shouldn't do this)
var ((i, j), k, l) = (1, 2, (3, 4)), m = 5; 
# i = 1
# j = 1
# k = 2
# l = (3, 4)
# m = 5
```

---

#### if statement

The if statement allows you to perform or not a certain operation based on a condition. It consists of the `if` keyword, parentheses `()` containing an expression and an embedded statement. It starts by evaluating the expression and implicitly casts the result as a [bool](#bool). If an implicit conversion does not exist, an exception is thrown. If the expression evaluates to `true`, the embedded statement is executed. You can also add the `else` keyword followed by a second embedded statement. This second statement is executed if the expression evaluated to `false`. You can also nest multiple if..else statements to check multiple conditions. The [ternary conditional operator](#ternary-conditional-operator) can also be used as an inline if.

```csharp
var temperature = 15; 

# if statement
if (temperature < 25)
    /echo 'cold'; 

# if..else statement
if (temperature < 25)
    /echo 'cold'; 
else
    /echo 'hot'; 

var character = 'A'; 

# 3 nested if..else statements
if (ord character in (ord 'A')..(ord 'Z' + 1)) {
    /echo 'uppercase letter'; 
} else if (ord character in (ord 'a')..(ord 'z' + 1)) {
    /echo 'lowercase letter'; 
} else if (ord character in (ord '0')..(ord '9' + 1)) {
    /echo 'digit'; 
} else {
    /echo 'symbol'; 
}
```

---

#### while, until, do..while and do..until statements

The while statement allows you to repeatedly perform an operation as long as a condition is true. It consists of the `while` keyword, parentheses `()` containing an expression and an embeded statement. It starts by evaluating the expression like an if statement. If the expression evaluated to `true`, the embeded statement is executed. Then it starts over again as long as the expression evaluates to true. If the expression is false to begin with, the operation is never executed. The until statement is the opposite of the while statement. It will repeatedly perform an operation as long as its condition is false. Its syntax is the same as the while statement but with the `until` keyword.

```csharp
var i = 0; 
while (i < 5) {
    /echo $i;
    i++;
}

var j = 0
until (j == 5) {
    /echo $j;
    j++;
}
```

Both while and until statements can be combined with the `do` keyword to make a do..while or do..until statement respectively. These statements act the same as their counterpart, except that their condition is evaluated after the operation. This means that their operation will always be executed at least once. These statements consists of the `do` keyword, an embedded statement, either the `while` or `until` keyword, parentheses `()` containing an expression and finally a semicolon `;`.

```csharp
var i = 5; 
do { # executed once
    /echo $i;
    i++;
} while (i < 5); 

var j = 5; 
do { # executed once
    /echo $j;
    j++;
} until (j == 5); 
```

---

#### for statement

The for statement is similar to the while statement, but provides additional functionalities which are especially useful for iterating over a range of numbers with a given increment. It consists of the `for` keyword, parentheses `()` and an embedded statement. Inside the parentheses, there are 3 expressions which are separated by 2 semicolons `;`.

The first expression is the initializer and is evaluated once at the very beginning. This is usually used to declare a variable with the let operator.

The second expression is the condition and acts like the condition of a [while statement](#while-until-dowhile-and-dountil-statements). The operation defined by the embedded statement will be executed as long as this expression is true.

The third expression is the increment and is evaluated after each iteration. This is usually used to increment the variable declared inside the initializer.

All 3 parts are optional and the condition defaults to `true` if it is omitted.

```csharp
var arr = { 1, 2, 3, 4, 5, 6, 7, 8, 9 }; 

for (let i = 0; i < len arr; i++) {
    var item = arr[i];
    /echo $item;
}

for(;;); # Infinite loop
```

---

#### for..in statement

The for..in statement allows you to execute an operation for each element of a collection. It consists of the `for` keyword, parentheses `()` and an embedded statement. Inside the parentheses, there are 2 parts separated by the `in` keyword. On the left, you have to provide an identifier or a tuple of identifiers. At each iteration, these identifiers will be assigned the next value inside the collection. On the right, you have to provide an expression which will be evaluated once. This expression should be an array, a string or a range. You are allowed to modify the collection during an iteration, but the changes will not directly affect the for..in statement, since the collection gets only evaluated once. Generally, this is cleaner than a [for statement](#for-statement) for iterating over a collection.

```csharp
var arr = { 1, 2, 3, 4, 5, 6, 7, 8, 9 }; 

for (item in arr)
    /echo $item; 
```

---

#### repeat statement

The repeat statement allows you to repeat an operation a given number of times. It consists of the `repeat` keyword, parentheses `()` containing an expression and an embedded statement. It evaluates the expression once and implicitly casts the result as a number. If an implicit conversion does not exist, an exception is thrown. The embedded statement is executed a number of times equivalent to that number.

```csharp
repeat (10)
    /echo 'Hello world'; 
```

---

#### loop statement

The loop statement allows you to repeat an operation an infinite number of times. It consists of the `loop` keyword and an embedded statement. The only way to stop a loop statement is with a [break](#break-statement), [return](#return-statement), [exit](#exit-statement) or [throw](#throw-statement) statement.

```csharp
# These 3 statements are infinite loops
loop; 
for (;;); 
while (true);
```

---

#### try statement

The try statement allows you to execute an operation and catch any exceptions that could be thrown. It consists of the `try` keyword and an embedded statement. It starts by trying to execute its embedded statement. If an exception is thrown, it catches it and the program resumes its execution after the try statement.

```csharp
# trys to declare variable foo
try let foo = { 1, 2, 3 }; 
```

You can add the `catch` keyword and another embedded statement which will be executed only if an exception is thrown. Between the `catch` keyword and its embedded statement, you can optionally add parentheses `()` containing an identifier. This identifier can then be used to retrieve the exception thrown from the catch's embedded statement.

```csharp
var tryParseNumber = (input, output) {
    try {
        val output = input as number; 
        return true;
    } catch {
        val output = null;
        return false;
    }
}; 
```

You can also add the `finally` keyword and another embedded statement which will be executed no matter what. You can use it if something needs to be executed whether an exception is thrown or not. You can also use it to overwrite a return or exit statement.

```csharp
var someFunction = () {
    exit;
}; 

try {
    try
        someFunction(); 
    finally
        throw; # overwrites the exit with an exception that is instantly caught preventing the program from exiting
}
```

If both a `catch` and a `finally` are provided, the catch has to be before the finally.

```csharp
/file text open read '''c:\users\public\test.txt''' |> set file; 

try
    /get file |> file text read_to_end |> echo; 
catch
    /echo 'Error reading file'; 
finally
    /get file |> file text close; 
```

---

#### lock statement

The lock statement allows you to lock variables, so that other asynchronous functions can't access it while you use it. It consists of the `lock` keyword, parentheses `()` containing an expression and an embedded statement. If you have multiple asynchronous functions modifying a shared variable, it is imperative that you use a lock statement to prevent them from modifying the same variable at the same time. Failing to do so will produce undefined behaviour.

```csharp
var (input, output) = array(); 

var workerThread = async () {
    while (true) {
        var value;

        lock (input) {
            if (len input == 0)
                continue;

            value = intput[^1];
            input = input[..^1];
        }

        # process the value;

        lock (output)
            output += value;
    }
}; 

repeat(5)
    workerThread(); 

lock (input)
    input = { 1, 2, 3, 4, 5 }; 

# 5 worker threads are now processing the data
```

---

#### continue statement

The continue statement allows you to end the current iteration of a loop and continue to the next one.

```csharp
for (i in ..5) {
    if (i == 2)
        continue;

    /echo $i;
}
# Output
# 0
# 1
# 3
# 4
```

---

#### break statement

The break statement allows you to end an entire loop.

```csharp
for (i in ..5) {
    if (i == 2)
        continue;

    /echo $i;
}
# Output
# 0
# 1
```

---

#### return statement

The return statement allows you to end the execution of a function and specify the value the function should return.

```csharp
var add = (a, b) {
    return a + b;
}; 
```

---

#### exit statement

The exit statement allows you to end the execution of the entire program and specify the value it should return.

```csharp
var stopProgramm = () {
    # clean up
    exit 0;
}; 
```

---

#### throw statement

The throw statement allows you to throw an exception, stopping the execution of everything until it is caught by a try statement or crashes the program.

```csharp
var add = (a, b) {
    if (a is not number || b is not number)
        throw `{nameof a} and {nameof b} must be numbers`;

    return a + b;
}; 
```

---

#### goto statement

The goto statement allows you to jump to a statement with a given label and continue executing a function from there.

```csharp
var arr = {
    { 1, 2, 3 },
    { 4, 5, 6 },
    { 7, 8, 9 }
}; 

var target; 

for (let i = 0; i < len arr; i++) {
    for (let j = 0; j < len arr[0]; j++) {
        if (arr[i][j] == 5) {
            target = (i, j);
            goto End;
        }
    }
}

End :: ;
```

---

### Commands

Commands are the only way to interact with resources outside of Bloc. They are not standardized and can be different in each implement and each platform. You should refer to the implementors website to know all available commands. Commands do not follow the same syntax as the rest of the language. They are defined by a name and a list of arguments separated by spaces. If you want an argument to contain spaces, you have to surround it with quotes to make a `string`. Every types of string literal can be used inside of commands. You can pass variables as arguments to command with the variable identifier `$`. They will be implicitly cast as strings. A variable can be interpreted as more than one argument if it contains spaces. You should use interpolated strings to guarantee that a variable will be a single argument.

```csharp
/echo Hello; 

/echo 'Hello world'; 

var command = 'echo Hello'; 
/$command; 

var text = 'Hello world'; 
/echo `{text}`; 
```

---

### Comments

You can use comments to write text that will not be evaluated. You can use this to explain code or to temporarily prevent the execution of a part of the code. A single line comment with a `#` and ends at the end of the line. A multi-line comment starts with a `#*` and end with a `*#`.

```csharp
/echo 'Hello world'; # This is a comment

#* This is a
multi-line comment *#
```
