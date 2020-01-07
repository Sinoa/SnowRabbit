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

### constant_types
    : 'int'
    | 'number'
    | 'string'

### non_void_types
    : 'int'
    | 'number'
    | 'string'
    | 'object'
    | 'bool'

### types
    : 'void'
    | non_void_types

### return_type
    : types


## Pre-Processor directive syntax

### directive
    : '#' directives

### directives
    : link_object_directive
    | script_compile_directive
    | constant_define_directive

### link_object_directive
    : 'link' '<string>'

### script_compile_directive
    : 'compile' '<string>'

### constant_define_directive
    : 'const' constant_types constant_var_name constant_value

### constant_var_name
    : '<identifier>'

### constant_value
    : '<integer>'
    | '<number>'
    | '<string>'


## Peripheral syntax

### peripheral_declare
    : 'using' peripheral_function_name '=' return_type import_peripheral_name '.' import_peripheral_function_name '(' [type_list] ')' ';' 

### peripheral_function_name
    : '<identifier>'

### import_peripheral_name
    : '<identifier>'

### import_peripheral_function_name
    : '<identifier>'

### type_list
    : non_void_types { ',' non_void_types }


## GlobalVariable syntax

### global_variable_declare
    : 'global' non_void_types global_var_name [ '=' expression ] ';'

### global_var_name
    : '<identifier>'


## Function declare syntax

### function_declare
    : 'function' return_type function_name '(' [argument_list] ')' { block } 'end'

### function_name
    : '<identifier>'

### argument_list
    : argument { ',' argument }

### argument
    : non_void_types argument_name

### argument_name
    : '<identifier>'


## Block syntax

### block
    : statement_list

### statement_list
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
    : 'local' non_void_types local_var_name [ '=' expression ] ';'

### local_var_name
    : '<identifier>'


## For statement syntax

### for_statement
    : 'for' '(' [for_initializer] ';' [for_condition] ';' [for_iterator] ')' { block } 'end'

### for_initializer
    : expression

### for_condition
    : expression

### for_iterator
    : expression


## While statement syntax

### while_statement
    : 'while' '(' while_condition ')' { block } 'end'

### while_condition
    : expression


## If statement syntax

### if_statement
    : 'if' '(' if_condition ')' { block } { 'else' 'if' '(' if_condition ')' { block } } 'end'

### if_condition
    : expression


## Break statement syntax

### break_statement
    : 'break' ';'


## Return statement syntax

### return_statement
    : 'return' [expression] ';'


## Expression syntax

### expression
    : simple_expression

### simple_expression
    : assignment_expression

### assignment_expression
    : condition_or_expression
    | unary_expression '=' expression
    | unary_expression '+=' expression
    | unary_expression '-=' expression
    | unary_expression '*=' expression
    | unary_expression '/=' expression
    | unary_expression '&=' expression
    | unary_expression '|=' expression
    | unary_expression '^=' expression

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
    : primary_expression
    | '+' unary_expression
    | '-' unary_expression
    | '!' unary_expression

### primary_expression
    : constant_var_name
    | paren_expression
    | global_var_name
    | local_var_name
    | argument_name
    | literal
    | function_call

### paren_expression
    : '(' expression ')'

### literal
    : '<integer>'
    | '<number>'
    | '<string>'
    | 'true'
    | 'false'
    | 'null'

### function_call
    : function_name '(' [parameter_list] ')'
    | import_peripheral_function_name '(' [parameter_list] ')'

### parameter_list
    : parameter { ',' parameter }

### parameter
    : expression

*/

namespace SnowRabbit.Compiler.Parser
{
    /// <summary>
    /// SnowRabbit の構文を解析を行う構文解析クラスです
    /// </summary>
    public class SrParser
    {
    }
}