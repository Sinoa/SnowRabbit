// zlib/libpng License
//
// Copyright(c) 2019 Sinoa
//
// This software is provided 'as-is', without any express or implied warranty.
// In no event will the authors be held liable for any damages arising from the use of this software.
// Permission is granted to anyone to use this software for any purpose,
// including commercial applications, and to alter it and redistribute it freely,
// subject to the following restrictions:
//
// 1. The origin of this software must not be misrepresented; you must not claim that you wrote the original software.
//    If you use this software in a product, an acknowledgment in the product documentation would be appreciated but is not required.
// 2. Altered source versions must be plainly marked as such, and must not be misrepresented as being the original software.
// 3. This notice may not be removed or altered from any source distribution.

/*

# 雪兎のスクリプト構文

## Compile unit syntax

### compile_unit
    : { directive }
    | { peripheral_declare }
    | { global_variable_declare }
    | { function_declare }


## Common syntax

### non_void_types
    : 'int'
    | 'number'
    | 'string'
    | 'object'
    | 'bool'

### types
    : 'void'
    | 'int'
    | 'number'
    | 'string'
    | 'object'
    | 'bool'


## Pre-Processor directive syntax

### directive
    : '#' directives

### directives
    : link_object_directive
    | script_compile_directive
    | constant_define_directive

### link_object_directive
    : 'link' <string>

### script_compile_directive
    : 'compile' <string>

### constant_define_directive
    : 'const' constant_types <identifier> constant_value

### constant_types
    : 'int'
    | 'number'
    | 'string'

### constant_value
    : <integer>
    | <number>
    | <string>


## Peripheral syntax

### peripheral_declare
    : 'using' <identifier> '=' types <identifier> '.' <identifier> '(' [type_list] ')' ';' 

### type_list
    : non_void_types { ',' non_void_types }


## GlobalVariable syntax

### global_variable_declare
    : 'global' non_void_types <identifier> ';'

## Function declare syntax

### function_declare
    : 'function' types <identifier> '(' [argument_list] ')' { block } 'end'

### argument_list
    : argument { ',' argument }

### argument
    : non_void_types <identifier>


## Block syntax

### block
    : statement

### statement
    : ';'
    | local_var_declare
    | for_statement
    | while_statement
    | if_statement
    | break_statement
    | return_statement
    | expression ';'


## LocalVariable declare syntax

### local_var_declare
    : 'local' non_void_types <identifier> [ '=' expression ] ';'


## For statement syntax

### for_statement
    : 'for' '(' [expression] ';' [expression] ';' [expression] ')' { block } 'end'


## While statement syntax

### while_statement
    : 'while' '(' expression ')' { block } 'end'


## If statement syntax

### if_statement
    : 'if' '(' expression ')' { block } { 'else' 'if' '(' if_condition ')' { block } } 'end'


## Break statement syntax

### break_statement
    : 'break' ';'


## Return statement syntax

### return_statement
    : 'return' [expression] ';'


## Expression syntax

### expression
    : assignment_expression

### assignment_expression
    : condition_or_expression
    | assignment_expression '=' condition_or_expression
    | assignment_expression '+=' condition_or_expression
    | assignment_expression '-=' condition_or_expression
    | assignment_expression '*=' condition_or_expression
    | assignment_expression '/=' condition_or_expression
    | assignment_expression '&=' condition_or_expression
    | assignment_expression '|=' condition_or_expression
    | assignment_expression '^=' condition_or_expression

### condition_or_expression
    : condition_and_expression
    | condition_or_expression { '||' condition_and_expression }

### condition_and_expression
    : or_expression
    | condition_and_expression { '&&' or_expression }

### or_expression
    : exclusive_or_expression
    | or_expression { '|' exclusive_or_expression }

### exclusive_or_expression
    : and_expression
    | exclusive_or_expression { '^' and_expression }

### and_expression
    : equality_expression
    | and_expression { '&' equality_expression }

### equality_expression
    : relational_expression
    | equality_expression { '==' relational_expression }
    | equality_expression { '!=' relational_expression }

### relational_expression
    : shift_expression
    | relational_expression { '<' shift_expression }
    | relational_expression { '>' shift_expression }
    | relational_expression { '<=' shift_expression }
    | relational_expression { '>=' shift_expression }

### shift_expression
    : addsub_expression
    | shift_expression { '<<' addsub_expression }
    | shift_expression { '>>' addsub_expression }

### addsub_expression
    : muldiv_expression
    | addsub_expression { '+' muldiv_expression }
    | addsub_expression { '-' muldiv_expression }

### muldiv_expression
    : unary_expression
    | muldiv_expression { '*' unary_expression }
    | muldiv_expression { '/' unary_expression }

### unary_expression
    : function_call
    | '+' unary_expression
    | '-' unary_expression
    | '!' unary_expression

### function_call
    : primary_expression
    | primary_expression '(' [parameter_list] ')'

### primary_expression
    : literal
    | <identifier>
    | paren_expression

### literal
    : <integer>
    | <number>
    | <string>
    | 'true'
    | 'false'
    | 'null'

### paren_expression
    : '(' expression ')'

### parameter_list
    : expression { ',' expression }

*/

using SnowRabbit.Compiler.IO;
using SnowRabbit.Compiler.Lexer;
using SnowRabbit.Compiler.Parser.SyntaxNodes;

namespace SnowRabbit.Compiler.Parser
{
    /// <summary>
    /// SnowRabbit の構文を解析を行う構文解析クラスです
    /// </summary>
    public class SrParser
    {
        private readonly ISrScriptStorage scriptStorage;
        private GlobalCompileContext compileContext;



        public SrParser(ISrScriptStorage storage)
        {
            scriptStorage = storage;
            compileContext = new GlobalCompileContext();
        }


        public void Compile(string scriptName)
        {
            var stream = scriptStorage.OpenRead(scriptName);
            var lexer = new SrLexer();
            lexer.Reset(stream);
            var localContext = new LocalCompileContext(lexer, compileContext);
            compileContext.PushCompileUnitContext(localContext);


            localContext.Lexer.ReadNextToken();
            var rootNode = CompileUnitSyntaxNode.Create(localContext);
        }
    }
}