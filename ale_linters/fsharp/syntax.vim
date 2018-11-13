if exists('g:loaded_fsharp_ale_linter')
    finish
endif
let g:loaded_fsharp_ale_linter = 1

if !has('python3') && !has('python')
    finish
endif

function! ale_linters#fsharp#syntax#Handle(buffer, lines) abort
    return fsharpbinding#python#CurrentErrors()
endfunction
"
call ale#linter#Define('fsharp',{
            \   'name': 'syntax',
            \   'executable': 'mono',
            \   'command': 'true',
            \   'callback': 'ale_linters#fsharp#syntax#Handle',
            \})

" vim: set et sts=4 sw=4:
