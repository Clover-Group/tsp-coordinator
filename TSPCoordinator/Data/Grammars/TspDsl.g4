grammar TspDsl;

main_rule: trilean_expr;

trilean_expr:
	boolean_expr
	| trilean_expr K_FOR ((K_EXACTLY? time) | time_interval) (
		time_cond
		| times_cond
	)? (K_FROM_START)?
	| trilean_expr K_UNTIL boolean_expr (time_cond | times_cond)?
	| trilean_expr K_ANDTHEN trilean_expr
	| trilean_expr K_AND trilean_expr
	| trilean_expr K_OR trilean_expr
	| K_WAIT LPAR time COMMA trilean_expr RPAR
	| LPAR trilean_expr RPAR;

boolean_expr:
	boolean_const
	| boolean_func
	| comparison
	| K_NOT boolean_expr
	| boolean_expr K_AND boolean_expr
	| boolean_expr K_XOR boolean_expr
	| boolean_expr K_OR boolean_expr
	| LPAR boolean_expr RPAR;

numerical_expr: number;

expr:
	number
	| string
	| identifier
	| arithmetical_func
	| expr (MULT | DIV | (MINUS GT GT?)) expr
	| expr (PLUS | MINUS) expr
	| MINUS? LPAR expr RPAR;

comparison: expr comparison_operator expr;

equality_operator: EQ | NOT_EQ | NOT_EQ2;

comparison_operator:
	GT
	| LT
	| GT_EQ
	| LT_EQ
	| equality_operator;

time_cond: comparison_operator time | numeric_range time;

times_cond: comparison_operator times | numeric_range K_TIMES;

arithmetical_func:
	avg_func
	| avgof_func
	| minof_func
	| maxof_func
	| aligned_func
	| derivation_func
	| lag_func
	| abs_func
	| custom_func;

boolean_func:
	increasing_func
	| decreasing_func
	| custom_func
	| isnull_func;

custom_func: identifier LPAR expr RPAR;

avg_func: F_AVG LPAR expr COMMA time RPAR;

aligned_func: F_ALIGNED LPAR time COMMA expr RPAR;

derivation_func: F_DERIVATION LPAR expr RPAR;

increasing_func:
	F_INCREASING LPAR expr COMMA numerical_expr COMMA numerical_expr RPAR;

decreasing_func:
	F_DECREASING LPAR expr COMMA numerical_expr COMMA numerical_expr RPAR;

avgof_func: F_AVGOF LPAR expr (COMMA expr)* RPAR;

minof_func: F_MINOF LPAR expr (COMMA expr)* RPAR;

maxof_func: F_MAXOF LPAR expr (COMMA expr)* RPAR;

lag_func: F_LAG LPAR expr (COMMA time)? RPAR;

abs_func: F_ABS LPAR expr RPAR;

isnull_func: F_ISNULL LPAR expr RPAR;

time_token:
	numerical_expr (K_MS | K_SEC | K_MIN | K_HR | K_DAY);

time: (time_token)+;

time_interval:
	time_range
	| time_with_abs_tol
	| time_with_rel_tol;

time_range: numeric_range (K_MS | K_SEC | K_MIN | K_HR | K_DAY);

time_with_abs_tol: time PM time;

time_with_rel_tol: time PM numerical_expr PERCENT;

times: numerical_expr K_TIMES;

numeric_range: numerical_expr K_TO numerical_expr;

boolean_const: K_TRUE | K_FALSE;

number: NUMBER;

string: STRING;

identifier: IDENTIFIER | DOUBLEQUOTED;

K_NOT: N O T;
K_AND: A N D;
K_OR: O R;
K_XOR: X O R;
K_ANDTHEN: A N D T H E N;
K_FROM_START: F R O M S T A R T;

K_UNTIL: U N T I L;
K_FOR: F O R;
K_EXACTLY: E X A C T L Y;
K_TIMES: T I M E S;
K_TO: T O;
K_WAIT: W A I T;

K_MS: (M S) | (M I L L I S E C O N D S?);
K_SEC: (S E C) | (S E C O N D S?);
K_MIN: (M I N) | (M I N U T E S?);
K_HR: (H R) | (H O U R S?);
K_DAY: (D A Y S?);

K_TRUE: T R U E;
K_FALSE: F A L S E;

F_LAG: L A G;
F_AVG: A V G;
F_AVGBY: A V G B Y;
F_MINBY: M I N B Y;
F_MAXBY: M A X B Y;
F_AVGOF: A V G O F;
F_MINOF: M I N O F;
F_MAXOF: M A X O F;
F_INCREASING: I N C R E A S I N G;
F_DECREASING: D E C R E A S I N G;
F_DERIVATION: D E R I V A T I O N;
F_ABS: A B S;
F_EACH: E A C H;
F_ANY: A N Y;
F_ALIGNED: A L I G N E D;
F_ISNULL: I S N U L L;

NUMBER: MINUS? DIGIT+ (DOT DIGIT+)?;

STRING: SQUOTE ( ~'\'' | SQUOTE SQUOTE)* SQUOTE;

DOUBLEQUOTED: DQUOTE ( ~'"' | DQUOTE DQUOTE)* DQUOTE;

IDENTIFIER: LETTER (LETTER | DIGIT | UNDERSCORE)*;

COMMA: ',';
DOT: '.';
LPAR: '(';
RPAR: ')';
LSQPAR: '[';
RSQPAR: ']';
MINUS: '-';
PLUS: '+';
MULT: '*';
DIV: '/';
LT: '<';
GT: '>';
EQ: '=';
NOT_EQ: '<>';
NOT_EQ2: '!=';
LT_EQ: '<=';
GT_EQ: '>=';
QUESTION: '?';
AMP: '&';
DOLLAR: '$';
AT: '@';
PERCENT: '%';
PM: '+-';

WS: [ \t\r\n]+ -> skip;

fragment SQUOTE: '\'';
fragment DQUOTE: '"';
fragment COLON: ':';
LBRACE: '{';
RBRACE: '}';
fragment LETTER: [a-zA-Z];
fragment UNDERSCORE: '_';
fragment DIGIT: [0-9];
fragment A: [aA];
fragment B: [bB];
fragment C: [cC];
fragment D: [dD];
fragment E: [eE];
fragment F: [fF];
fragment G: [gG];
fragment H: [hH];
fragment I: [iI];
fragment J: [jJ];
fragment K: [kK];
fragment L: [lL];
fragment M: [mM];
fragment N: [nN];
fragment O: [oO];
fragment P: [pP];
fragment Q: [qQ];
fragment R: [rR];
fragment S: [sS];
fragment T: [tT];
fragment U: [uU];
fragment V: [vV];
fragment W: [wW];
fragment X: [xX];
fragment Y: [yY];
fragment Z: [zZ];