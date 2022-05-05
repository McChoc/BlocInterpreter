# C-- Interpretor
---

## Table of content

- [Example project](#example-project)
- [Fundamentals](#fundamentals)
- [Language reference](#language-reference)
    - [Values](#values)
    - [Variables](#variables)
    - [Operators](#operators)
    - [Statements](#statements)
    - [Commands](#commands)
    - [Comments](#comments)

## Example project

## Fundamentals

## Language reference

### Values

#### void

You can use the `void` literal to create a `void` value. It cannot be assigned to a variable and is only used when a function returns nothing. However the return statement implicitly returns `void` when no value is provided, so the `void` literal is rarely used. Since this type can only have one value, the `void` literal can be used both as a value and as a type.

```python
def display = (num) {
    if (num is not number) # if num is not a number
        return;         # returns void

    /echo $num;
}; 

def foo = void; # throws an exception

void is void;   # evaluates to true
```

---

#### null

You can use the `null` literal to create a `null` value. This is used to represent that a variable contains nothing and it is the default value of any variable. Since this type can only have one value, the `null` literal can be used both as a value and as a type.

```python
def foo;        # foo has a value of null

null is null;   # evaluates to true
```

---

#### bool

A `bool` is used to represent a boolean value which can be true or false. You can use the `true` and `false` literals to create a `bool` value. They are mainly used as conditions inside `if` statements and loops.

```python
def condition = true; 

if (condition)
    /echo 'Hello world'; 

typeof true;    # evaluates to bool
typeof false;   # evaluates to bool
```

---

#### number

A `number` is used to represent both integers and floating-point numbers. To create an integer, you can simply write a decimal number with digits from 0 to 9 or you can prefix it with `0b`, `0o` or `0x` to write a number in binary, octal or hexadecimal respectivly. To create a floating point number, you can simply add a dot `.` to separate its integral and fractional parts. Floating point numbers have to be decimal, but you can use the scientific notation by adding an `e` and an exponent. Both integers and floating-point literals can contain any number of underscores `_` to separate digits. You can also use the `infinity` and `nan` literals to create a number.

```python
# they all represent the same number
42; 
0b_0010_1010; 
0o52; 
0x2A; 

3.141_592_654; 
0.42E-2; 
```

---

#### range

You can create a `range` using the [range operator `..`](#range-operator) and optionaly numbers. Ranges can be used to get a slice of a [string](#string) or [array](#array). They can also be used in a [for..in](#forin-statement) loop to iterate over all the integers inside that range. When iterating over a range, if the first number is not omited, it defaults to 0. The second number defaults to `infinity` or `-infinity` depending on the sign of the step. The default step is 1, but you can change it by adding a second range operator `..`. A step of 2 for example will iterate over every second iteger inside the range. A negative step will reverse the iteration. For more information on getting a slice of a string or array, see [indexer]().

```python
def arr = { 1, 2, 3 }; 
arr[..2];   # evaluates to { 1, 2 }

# outputs the numbers from 0 to 9
for (i in ..10) 
    /echo $i; 

typeof(..); # evaluates to range
```

---

#### string

A `string` is used to represent text. You can create one by putting a chain of characters between single `'` or double quotes `"`. Whether you use single or double quotes does no change anything at runtime, but using one allows you to create a string containing the other one. The string literal has to be on a single line.

```python
'this is a string'; 
"this is also a string"; 

''  # this is an empty string

'Hello' == "Hello"; # evaluates to true

'this "string" contains double quotes (")'; 
"this 'string' contains single quotes (')"; 
```

If you want a string to include both single and double quotes or simply include special characters, you can use escape sequences with the backslash `\` symbol.

| Escape sequance | Character name  | Unicode encoding |
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
'''\ '''''; # evaluates to "\\ \'"

"""
Hello 
world
"""; # evaluates to '\nHello\nworld\n'
```

You can also create a string with back quotes ``` ` ```. These types of string are called interpolated string. They allow you to embed expressions inside curly brackets `{ }` within a string. The content of the curly brackets will be evaluated and implicitly converted to a string before being concatenated to the rest of the string at runtime. This improves readability over concatenating multiple expressions with the [addition operator `+`](#addition-operator). If you want to actualy have curly brackets inside an interpolated string, you have to double them.

```python
def foo = 42; 

# both evaluates to 'foo: 42'
nameof foo + ': ' + foo; 
`{nameof foo}: {foo}`; 

`{foo} {{foo}} {{{foo}}}`; # evaluates to '42 {foo} {42}'
```

You can also combine raw strings and interpolated strings with triple back quotes ` ``` `.

````python
def foo = 42; 

```
{foo}
```; # evaluates to '\n42\n'
````

---

#### array

An `array` is used to represent a list of ordered values. You can create one by separating values with comams `,` inside braces `{ }`. The values can be of any type and you are allowed to leave a trailing comma `,` after the last value. You cannot make an empty array literal because it would be ambiguus with an empty `struct`. 

```python
def myArray = { 1, 2, 3 }; 
```

---

#### struct

A `struct` is used to represent a set of key-value pairs. You can create one by separating key-value pairs with commas `,` inside braces `{ }`. A key-value pair is formed by a name, the equal symbol `=` and a value. The values can be of any type and the keys have to be valid variable names. You are allowed to leave a trailing comma `,` after the last key-value pair. You cannot make an empty struct literal because it would be ambiguus with an empty `array`. 

```python
def myStruct = {
    a = 1,
    b = 2,
    c = 3
}; 
```

---

#### tuple

You can create a `tuple` by separating two or more expressions with comas `,`. Tuples are also usualy surouned by parentheses `()` because of the [operator precedence](). Tuples are useful to perform an operation on multiple values at the same time. You can perform an operation between all the values of a tuple and a single value or between all the values of two tuples. If the tuple don't have the same size, an exception is thrown.

```python
1, 2, 3;        # evaluates to (1, 2, 3)

`{1, 2, 3}`;    # evaluates to '(1, 2, 3)'


(1, 2) + 1;         # evaluates to (2, 3)

(1, 2) + (3, 4);    # evaluates to (4, 6)

(1, 2) + (3, 4, 5); # throws an exception
```

---

#### function

A `function` is used to hold statements that you want to execute at more than one place and allows you to reuse code. You can generalize a function by using parameters, allowing you to use that function in even more places. There are two syntax you can use to create a `function`. With the first syntax, you have to put a pair of parentheses `()` next to a pair of braces `{ }`. The parentheses can contain a list of comma `,` separated identifiers. These are the parameters of the function. The braces can contain a list of statements to execute when the function is called. These statements can acces the parameters of the function directly by using the names of the parameters as variables or by using the `params` keyword.

```python
def print = (text) {
    /echo $text;
}; 
```

The second syntax is useful for single line function. To create a function using this syntax, you have to put a pair of parentheses next to the lambda operator `=>` next to an expression. The parentheses can be omited if there is exactly one parameter. This type of function will evaluate its expression and return the result. 

```python
def add = (a, b) => a + b; 

def toString = x => x as string; 
```

Both syntax can be used to create asynchronus functions by prefixing them with the `async` keyword. Asynchronus functions allow you to run slow code on a new thread without blocking the main thread. They return a `task` that can be used to wait for the operation to end or retreive the value returned by the function.

```python
def print = async (text) {
    /echo $text;
}; 

def toString = async x => x as string; 
```

---

#### task

Tasks cannot be directly created. They are returned when calling an `async` function. You can use the `await` operator to retreive the value returned by the function. If the function is not done executing, it will block the thread until it is before returning the result.

```python

```

---

#### reference

You can create a reference to a variable by using the `ref` operator on it. You can also use the `new` operator to simultaniously create a variable on the heap and return a reference to it. References are useful if you want multiple variables to point to the same value.

```python
def foo = { 1, 2 }; 

def ref1 = ref foo;         # ref1 is a reference to the variable foo

def ref2 = new { 1, 2 };    # ref2 is a reference to an array stored on the heap
```

---

#### complex

Complexes cannot be directly created. They are returned by some command and are essentialy references to objects that lives outside of C--. You cannot modify them in any way, you can only store them and pass them to an other command.

---

#### type

You can create a type by using any of the following literals : `bool`,  `number`,  `range`,  `string`,  `array`,  `struct`,  `tuple`,  `function`,  `task`,  `reference`,  `complex`,  `type` and  `any`. While they are not type literals, `void` and `null` can be used as such because there is an implicit conversion from both `void` and `null` to `type`. You can also use the `typeof` operator to get the type of a value. Types can be used to check if a value is of a certain type with the `is` operator. Types can be combined into composit types using bitwise operators allowing to to check if a value is of one of many types at the same time. `any` is the composite type of every types. Types can also be used to cast a value from its type to another with the `as` operator. A value cannot be casted to a composite type.

```python

```

---

### Variables

Variables allow you to store values and associate them with a name. A variable name can contain both lower and upper case characters, digits, uderscores `_` and any special character except for these : ```!"#$%&'()*+,-./:;<=>?@[\]^`{|}~```. A variable name cannot contain white spaces and cannot start with a digit. You can create a variable using the `def` statement. You can then access its value with its name.

```python
# declaring a variable foo
def foo; 

# asigning it 'Hello world'
foo = 'Hello world'; 

# retreiving its value
foo; 

# you can declare a variable and asign it a value at the same time
def bar = 42; 
```

Some valid variable names can be embiguus between a variable and a keyword. By default, the interpretor will interpret them as keywords, but you can use the variable identifier character `$` to force the interpretor to interpret them as variables. The `$` **is not** part of the variable name. This means that the `nameof` operator will return the name with out the `$` and when using indexers you have to ommit the `$`. You can use the variable identifier character with any variable even if they are not embiguus and you have to use it with every variable inside a command.

```python
def $for = { 'a', 'b', 'c' };

for (item in $for)
    /echo $item;

nameof $for; # evaluates to 'for'

def foo = {
    $if = true
};

# both point to the same variable
foo.$if;
foo['if'];

def bar = 42;

$bar; # evaluates to 42
```
---

### Operators

Operators allow you to perform basic operations on values and variables. You can combine values and variables with operators to create complexe expressions. An expression can then be evaluated and reduced to a single value which can be used by statements. The order in which operators are evaluated is determined by the precedence and associativity of these operators, but you can use parentheses `()` to change that order.

The following table shows the precedence of all operators. The operators at the top of the table are evaluated before the ones at the bottom. If some operators have the same precedence, their associativity will determine the order of operation. 

| Operators                                                                                | Description          | Associativity    |
|------------------------------------------------------------------------------------------|----------------------|------------------|
| [`.`](#member-access-operator), [`[]`](#indexer-operator), [`()`](#invocation-operator)  | Primary              | Left-to-right 🡲 |
| [`+`](#unary-plus-operator), [`-`](#unary-minus-operator-), [`~`](#complement-operator), [`!`](#negation-operator), [`++`](#increment-operator), [`--`](#decrement-operator-), [`~~`](#variable-complement-operator), [`!!`](#variable-negation-operator), [`len`](#length-operator-len), [`chr`](#character-operator-chr), [`ord`](#ordinal-operator-ord), [`val`](#value-operator-val), [`ref`](#reference-operator-ref), [`new`](#allocation-operator-new), [`await`](#await-operator-await), [`nameof`](#nameof-operator-nameof), [`typeof`](#typeof-operator-typeof) | Unary | Right-to-left 🡰 |
| [`..`](#range-operator)                                                                  | Range                | Left-to-right 🡲 |
| [`**`](#power-operator), [`//`](#root-operator), [`%%`](#logarithm-operator)             | Exponential          | Left-to-right 🡲 |
| [`*`](#multiplication-operator), [`/`](#division-operator), [`%`](#remainder-operator)   | Multiplicative       | Left-to-right 🡲 |
| [`+`](#addition-operator), [`-`](#subtraction-operator-)                                 | Additive             | Left-to-right 🡲 |
| [`<<`](#left-shift-operator), [`>>`](#right-shift-operator)                              | Shift                | Left-to-right 🡲 |
| [`<=>`](#three-way-comparison-operator)                                                  | Three-way comparison | Left-to-right 🡲 |
| [`<`](#less-than-operator), [`>`](#greater-than-operator), [`<=`](#less-than-or-equal-operator), [`>=`](#greater-than-or-equal-operator), [`in`](#in-operator-in), [`not in`](#not-in-operator-not-in), [`is`](#is-operator-is), [`is not`](#is-not-operator-is-not), [`as`](#as-operator-as) | Relation | Left-to-right 🡲 |
| [`==`](#equality-operator), [`!=`](#inequality-operators), [`<>`](#inequality-operators) | Equality             | Left-to-right 🡲 |
| [`&`](#bitwise-and-operator)                                                             | Bitwise AND          | Left-to-right 🡲 |
| [`^`](#bitwise-xor-operator)                                                             | Bitwise XOR          | Left-to-right 🡲 |
| [`|`](#bitwise-or-operator)                                                              | Bitwise OR           | Left-to-right 🡲 |
| [`&&`](#boolean-and-operator)                                                            | Boolean AND          | Left-to-right 🡲 |
| [`^^`](#boolean-xor-operator)                                                            | Boolean XOR          | Left-to-right 🡲 |
| [`||`](#boolean-or-operator)                                                             | Boolean OR           | Left-to-right 🡲 |
| [`?:`](#ternary-conditional-operator)                                                    | Ternary              | Right-to-left 🡰 |
| [`=`](#assignment-operator), [`+=`, `-=`, `*=`, `/=`, `%=`, `**=`, `//=`, `%%=`, `<<=`, `>>=`, `&=`, `|=`, `^=`, `&&=`, `||=`, `^^=`](#compound-assignment-operators) | Assignment | Right-to-left 🡰 |
| `=>`                                                                                     | Lambda               | Right-to-left 🡰 |
| `,`                                                                                      | Comma                | Left-to-right 🡲 |

The pipe operator `|>` is not in that table because it can only be used inside a command statement which cannot have any other operators. Its associativity is from left to right.

Unrelated to operator precedence and associativity, an operator will evaluate its operands from left to right.

Note that the operands don't need to be exactly the type specified in a certain context, they only need to be [implicitly convertible]() to that type. In the case were the operands are implicitly convertible to multiple types and match more than one context, the upper most one takes priority. In the case were the operands do not match any context, an exception is thrown.

---

#### Addition operator `+`

When both its operands are of type [number](#number), it will compute the sum of its operands.

```python
5 + 4.6; # evaluates to 9.6
```

When both its operands are of type [array](#array), it will concatenate its operands.

```python
{ 4, 4 } + { 2, 7 }; # evaluates to { 4, 4, 2, 7 }
```

When its left operand is of type [array](#array), but the right one is not, it will append the right operand to the [array](#array).

```python
{ 2, 5 } + 2; # evaluates to { 2, 5, 2 }
```

When its right operand is of type [array](#array), but the left one is not, it will prepend the left operand to the [array](#array).

```python
2 + { 3, 8 }; # evaluates to { 2, 3, 8 }
```

When both its operands are of type [string](#string), it will concatenate its operands.

```python
'fizz' + 'buzz'; # evaluates to 'fizzbuzz'
```

Note that [string interpolation]() provides a more convenient way to format strings.

---

#### Subtraction operator `-`

When both its operands are of type [number](#number), it will subtract its right operand from its left operand.

```python
5 - 3;      # evaluates to 2
7 - 5.8;    # evaluates to 1.2
5 - 9;      # evaluates to -4
```

---

#### Multiplication operator `*`

When both its operands are of type [number](#number), it will compute the product of its operands.

```python
2 * 8.6; # evaluates to 17.2
```

When one of its operands is of type [array](#array) and the other is of type [number](#number), it will concatenate an empty array with the given array a number of time equivalent to the floor of the given number.

```python
{ 1, 2 } * 3;   # evaluates to { 1, 2, 1, 2, 1, 2 }
{ 1, 2 } * 0;   # evaluates to { }
{ 1, 2 } * 1.9; # evaluates to { 1, 2 }
{ 1, 2 } * -2;  # throws an exception
```

When one of its operands is of type [string](#string) and the other is of type [number](#number), it will concatenate an empty string with the given string a number of time equivalent to the floor of the given number.

```python
'ab' * 3;   # evaluates to 'ababab'
'ab' * 0;   # evaluates to ''
'ab' * 1.9; # evaluates to 'ab'
'ab' * -2;  # throws an exception
```

---

#### Division operator `/`

When both its operands are of type [number](#number), it will divide its left operand by its right operand. Dividing a positive number by zero will return `infinity`, dividing a negative number by zero will return `-infinity` and dividing zero by zero will return `nan`.

```python
6 / 4;  # evaluates to 1.5
1 / 0;  # evaluates to infinity
-1 / 0; # evaluates to -infinity
0 / 0;  # evaluates to nan
```

The `/` can also be used to make a [command statement]().

---

#### Remainder operator `%`

When both its operands are of type [number](#number), it will compute the remainder after dividing its left operand by its right operand. Even though it is similar, this **is not** a modulus operator. It can be used as a modulus operator as long as its operands are positive, but with negative values the result will not be the same.

```python
8 % 3;      # evaluates to 2
8.4 % 1;    # evaluates to 0.4
```

---

#### Power operator `**`

When both its operands are of type [number](#number), it will raise its left operand to the power of its right operand.

```python
5 ** 4;     # evaluates to 625
10 ** -1;   # evaluates to 0.1
25 ** 0.5;  # evaluates to 5
```

---

#### Root operator `//`

When both its operands are of type [number](#number), it will compute the n^th^ root of its left operand, where n is the value of its right operand.

```python
25 // 2; # evaluates to 5 (the square root of 25)
```

---

#### Logarithm operator `%%`

When both its operands are of type [number](#number), it will compute the logarithm of its left operand with its right operand as the base.

```python
100 %% 10; # evaluates to 2 (the logarithm base 10 of 100)
```

---

#### Unary plus operator `+`

When its operand is of type [number](#number), it will return the value of that operand. It can be used to [implicitly convert]() a value to a [number](#number).

```python
+4;     # evaluates to 4
+true;  # evaluates to 1
```

---

#### Unary minus operator `-`

When its operand is of type [number](#number), it will compute the numeric negation of its operand.

```python
-4;     # evaluates to -4
- -4;   # evaluates to 4
```

Note that there is a space between the two `-`. If we remove that space, they will be interpreted as the [decrement operator `--`](#decrement-operator-). 

---

#### Increment operator `++`

When its operand is of type [number](#number), it will increment its operand by 1. The operand must be a variable. This operator can be used both as a prefix and a sufix. When used as a prefix, it returns the value of the variable *after* the increment. When used as a sufix, it returns the value of the variable *before* the increment, but after the implicit conversion.

```python
def foo = 0; 
++foo;          # evaluates to 1 and foo has a value of 1

def bar = 0; 
bar++;          # evaluates to 0, but bar has a value of 1

0++;            # is a syntax error
```

---

#### Decrement operator `--`

When its operand is of type [number](#number), it will decrement its operand by 1. The operand must be a variable. This operator can be used both as a prefix and a sufix. When used as a prefix, it returns the value of the variable *after* the decrement. When used as a sufix, it returns the value of the variable *before* the decrement, but after the implicit conversion.

```python
def foo = 0; 
--foo;          # evaluates to -1 and foo has a value of -1

def bar = 0; 
bar--;          # evaluates to 0, but bar has a value of -1

0--;            # is a syntax error
```

---

#### Boolean AND operator `&&`

This operator will compute the logical AND of its operands. Its operands are implicitly converted to bools for the evaluation, but the result will not nececeraly be of type `bool`. The operator returns its left operand if it evaluates to `false` otherwise it returns its right operand. When using bools, the result is `true` if both its operands evaluate `true`. Otherwise the result is `false`. If its left operand evaluates to `false`, its right operand is not evaluated.

```python
def result;

def operand = (value, text) {
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

This operator will compute the logical OR of its operands. Its operands are implicitly converted to bools for the evaluation, but the result will not nececeraly be of type `bool`. The operator returns its left operand if it evaluates to `true` otherwise it returns its right operand. When using bools, the result is `true` if one of its operands evaluate `true`. The result is `false` if both its operands evaluate `false`. If its left operand evaluates to `true`, its right operand is not evaluated.

```python
def result;

def operand = (value, text) {
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

def foo = null; 
def bar = {
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

This operator will compute the logical XOR of its operands. The result is `true` if one of its operands evaluate `true` and the other evaluates to `false`. The result is `false` if both its operands evaluate `true` or if both its operands evaluate `false`.

```python
false ^^ false; # evaluates to false

false ^^ true;  # evaluates to true

true ^^ false;  # evaluates to true

true ^^ true;   # evaluates to false
```

---

#### Negation operator `!`

This operator will compute the logical negation of its operand.The result is `true`, if its operand evaluates to `false`, and `false`, if its operand evaluates to `true`.

```python
!true;  # evaluates to false
!false; # evaluates to true
```

---

#### Variable negation operator `!!`

This operator will compute the lopgical negation of its operand and assign it the result. The operand must be a variable. This operator can be used both as a prefix and a sufix. When used as a prefix, it returns the value of the variable *after* the negation. When used as a sufix, it returns the value of the variable *before* the negation, but after the implicit conversion.

```python
def foo = false; 
!!foo;          # evaluates to true and foo has a value of true

def bar = false; 
bar!!;          # evaluates to false, but bar has a value of true

def num = 1; 
num!!;          # evaluates to true, but num has a value of false

false!!;        # is a syntax error
```

---

#### Bitwise AND operator `&`

---

#### Bitwise OR operator `|`

---

#### Bitwise XOR operator `^`

---

#### Left-shift operator `<<`

---

#### Right-shift operator `>>`

---

#### Complement operator `~`

---

#### Variable complement operator `~~`

---

#### Equality operator `==`

---

#### Inequality operators `!=`, `<>`

---

#### Less than operator `<`

---

#### Greater than operator `>`

---

#### Less than or equal operator `<=`

---

#### Greater than or equal operator `>=`

---

#### Three-way comparison operator `<=>`

---

#### Assignment operator `=`

---

#### Compound assignment operators

`+=`, `-=`, `*=`, `/=`, `%=`, `**=`, `//=`, `%%=`, `<<=`, `>>=`, `&=`, `|=`, `^=`, `&&=`, `||=`, `^^=`

---

#### Member access operator `.`

---

#### Indexer operator `[]`

---

#### Invocation operator `()`

---

#### Range operator `..`

---

#### Ternary conditional operator `?:`

---

#### In operator `in`

---

#### Not in operator `not in`

---

#### Is operator `is`

---

#### Is not operator `is not`

---

#### As operator `as`

---

#### Length operator `len`

---

#### Character operator `chr`

---

#### Ordinal operator `ord`

---

#### Value operator `val`

---

#### Reference operator `ref`

---

#### Allocation operator `new`

---

#### Nameof operator `nameof`

---

#### Typeof operator `typeof`

---

#### Await operator `await`

---

#### Pipe operator `|>`

---

### Statements

##### Statements
##### Statement blocks
##### Embedded statements
##### Labeled statements

---

#### Expression statement

---

#### Command statement

---

#### Empty statement

---

#### pass statement

---

#### def statement

---

#### delete statement

---

#### if statement

---

#### try statement

---

#### lock statement

---

#### loop statement

---

#### repeat statement

---

#### for statement

---

#### for..in statement

---

#### while statement

---

#### until statement

---

#### do statement

---

#### continue statement

---

#### break statement

---

#### return statement

---

#### exit statement

---

#### throw statement

---

#### goto statement

---

### Commands

### Comments
