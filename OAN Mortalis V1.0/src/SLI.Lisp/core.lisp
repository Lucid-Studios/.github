(defpackage :sli-core
  (:use :cl)
  (:export :execute :*trace*))

(in-package :sli-core)

(defparameter *trace* '())

(defun execute (expr)
  (push expr *trace*)
  expr)
