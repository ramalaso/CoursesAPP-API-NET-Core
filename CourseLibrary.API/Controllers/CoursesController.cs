﻿using AutoMapper;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authors/{authorId}/courses")]
    public class CoursesController : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IMapper _mapper;

        public CoursesController(ICourseLibraryRepository courseLibraryRepository, IMapper mapper)
        {
            _courseLibraryRepository = courseLibraryRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CourseDto>> GetCoursesForAuthor(Guid authorId)
        {
            
            if(!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }
            var coursesForAuthorFromRepo = _courseLibraryRepository.GetCourses(authorId);
            return Ok(_mapper.Map<IEnumerable<CourseDto>>(coursesForAuthorFromRepo));
        }

        [HttpGet("{courseId}", Name ="GetCourseForAuthor")]
        public ActionResult<CourseDto> GetCourseForAuthor(Guid authorId, Guid courseId)
        {
            var courseForAuthorFromRepo = _courseLibraryRepository.GetCourse(authorId, courseId);

            if (!_courseLibraryRepository.AuthorExists(authorId) || courseForAuthorFromRepo == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map<CourseDto>(courseForAuthorFromRepo));
        }

        [HttpPost]
        public ActionResult<CourseDto> CreateCourseForAuthor(Guid authorId, CourseForCreationDto course)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var courseEntity = _mapper.Map<Entities.Course>(course);
            _courseLibraryRepository.AddCourse(authorId, courseEntity);
            _courseLibraryRepository.Save();

            var courseToReturn = _mapper.Map<CourseDto>(courseEntity);
            return CreatedAtRoute("GetCourseForAuthor", new { authorId = authorId, courseId = courseToReturn.Id }, courseToReturn);
        }

        [HttpPut("{courseId}")]
        public ActionResult UpdateCourseForAuthor(Guid authorId, Guid courseId, CourseForUpdateDto course) 
        {
            if (!_courseLibraryRepository.AuthorExists(authorId)) return NotFound();
            var courseForAuthorFromRepo = _courseLibraryRepository.GetCourse(authorId, courseId);
            if (courseForAuthorFromRepo == null) return NotFound();

            _mapper.Map(course, courseForAuthorFromRepo);
            _courseLibraryRepository.UpdateCourse(courseForAuthorFromRepo);
            _courseLibraryRepository.Save();
            return NoContent();
        }

        [HttpPatch("{courseId}")]
        public ActionResult PartiallyUpdateCourseForAuthor(Guid authorId, Guid courseId, JsonPatchDocument<CourseForUpdateDto> patchDocument)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId)) return NotFound();
            var courseForAuthorFromRepo = _courseLibraryRepository.GetCourse(authorId, courseId);
            if (courseForAuthorFromRepo == null) return NotFound();
            var coursePatch = _mapper.Map<CourseForUpdateDto>(courseForAuthorFromRepo);
            if (TryValidateModel(coursePatch))
            {
                return ValidationProblem(ModelState);
            }
            patchDocument.ApplyTo(coursePatch);
            _mapper.Map(coursePatch, courseForAuthorFromRepo);
            _courseLibraryRepository.UpdateCourse(courseForAuthorFromRepo);
            _courseLibraryRepository.Save();
            return NoContent();
        }
    }
}
